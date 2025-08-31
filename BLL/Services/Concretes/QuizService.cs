using BLL.DTO;

using Microsoft.EntityFrameworkCore;

using BLL.Services.Abstract;
using DAL.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using MODEL.Entities;

using System.Linq;
using System.Collections.Generic;
using System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BLL.DTO.QuizDtos;
using TimeZoneConverter;

namespace BLL.Services.Concretes
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _uow;
       

        public QuizService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<StartQuizResult> StartAsync(StartQuizRequest req, CancellationToken ct = default)
        {
            if (req.SeedCount < 1) req.SeedCount = 10;

            var seedIds = await SelectSeedEnglishWordIdsAsync(req.WordListId, req.SeedCount, ct);
            if (seedIds.Count == 0) throw new InvalidOperationException("Seçilen listede kelime yok.");

            var cycle = seedIds.ToList();
            ShuffleInPlace(cycle);

            var run = new QuizRun
            {
                WordListId = req.WordListId,
                Mode = req.Mode,
                SeedCount = seedIds.Count,
                SeedWordIdsCsv = string.Join(",", seedIds),
                CycleOrderCsv = string.Join(",", cycle),
                CycleIndex = 0,
                IsPractice = false,
                SourceRunId = null,
                StartedAt = DateTime.UtcNow
            };

            await _uow.QuizRuns.AddAsync(run, ct);
            await _uow.SaveChangesAsync(ct);

            return new StartQuizResult { QuizRunId = run.Id, WordListId = run.WordListId, Mode = run.Mode };
        }



        // 🔹 Basit seçim: Seçilen listenin kelimeleri arasından TimesShown en düşük olanı getirir (eşitlikte rastgele).
        public async Task<QuizQuestionDto?> GetNextQuestionAsync(int quizRunId, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct);
            if (run == null || string.IsNullOrWhiteSpace(run.SeedWordIdsCsv)) return null;

            var seed = (run.SeedWordIdsCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0).Where(id => id > 0).ToList();
            if (seed.Count == 0) return null;

            var cycle = (run.CycleOrderCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0).Where(id => id > 0).ToList();

            if (cycle.Count != seed.Count)
            {
                cycle = seed.ToList();
                ShuffleInPlace(cycle);
                run.CycleOrderCsv = string.Join(",", cycle);
                run.CycleIndex = 0;
                await _uow.SaveChangesAsync(ct);
            }

            if (run.CycleIndex >= cycle.Count)
            {
                var newOrder = seed.ToList();
                int safety = 0;
                do { ShuffleInPlace(newOrder); safety++; }
                while (safety < 5 && Enumerable.SequenceEqual(newOrder, cycle));

                cycle = newOrder;
                run.CycleOrderCsv = string.Join(",", cycle);
                run.CycleIndex = 0;
                await _uow.SaveChangesAsync(ct);
            }

            var targetId = cycle[run.CycleIndex];
            run.CycleIndex++;
            await _uow.SaveChangesAsync(ct);

            return await BuildQuestionDto(run.Mode, targetId, seed, ct);
        }



        private async Task<QuizQuestionDto> BuildQuestionDto(QuizMode mode, int targetId, List<int> seedIds, CancellationToken ct)
        {
            if (mode == QuizMode.EnglishToTurkish)
            {
                // Prompt = EN kelime, Doğru = primary TR (yoksa ilk)
                var en = await _uow.EnglishWords.Query().AsNoTracking()
                    .Where(w => w.Id == targetId)
                    .Include(w => w.Translations).ThenInclude(t => t.TurkishWord)
                    .FirstOrDefaultAsync(ct) ?? throw new InvalidOperationException("Word not found");

                var prompt = en.Text;
                var correct = en.Translations
                    .OrderByDescending(t => t.IsPrimary).ThenBy(t => t.Id)
                    .Select(t => t.TurkishWord.Text)
                    .FirstOrDefault() ?? "(çeviri yok)";

                // Çeldiriciler: önce seed içindeki TR'ler
                var seedTurkishPool = await _uow.WordTranslations.Query().AsNoTracking()
                    .Where(t => seedIds.Contains(t.EnglishWordId) && t.EnglishWordId != targetId)
                    .Select(t => t.TurkishWord.Text)
                    .Distinct()
                    .Take(200)
                    .ToListAsync(ct);

                // Yetersizse globalden tamamla
                if (seedTurkishPool.Count < 3)
                {
                    var globalPool = await _uow.WordTranslations.Query().AsNoTracking()
                        .Where(t => t.EnglishWordId != targetId)
                        .Select(t => t.TurkishWord.Text)
                        .Distinct()
                        .Take(400)
                        .ToListAsync(ct);
                    seedTurkishPool.AddRange(globalPool);
                }

                var options = MakeOptions(correct, seedTurkishPool, 3);
                return new QuizQuestionDto { EnglishWordId = targetId, Prompt = prompt, Options = options };
            }
            else // TurkishToEnglish
            {
                // Prompt = primary TR (yoksa ilk), Doğru = EN kelime
                var primaryTr = await _uow.WordTranslations.Query().AsNoTracking()
                    .Where(t => t.EnglishWordId == targetId)
                    .Include(t => t.TurkishWord)
                    .OrderByDescending(t => t.IsPrimary).ThenBy(t => t.Id)
                    .Select(t => t.TurkishWord.Text)
                    .FirstOrDefaultAsync(ct) ?? "(TR anlam yok)";

                var correct = await _uow.EnglishWords.Query().AsNoTracking()
                    .Where(w => w.Id == targetId)
                    .Select(w => w.Text)
                    .FirstOrDefaultAsync(ct) ?? "(EN yok)";

                // Çeldiriciler: önce seed içindeki EN'ler
                var seedEnglishPool = await _uow.EnglishWords.Query().AsNoTracking()
                    .Where(w => seedIds.Contains(w.Id) && w.Id != targetId)
                    .Select(w => w.Text)
                    .Distinct()
                    .Take(200)
                    .ToListAsync(ct);

                if (seedEnglishPool.Count < 3)
                {
                    var globalPool = await _uow.EnglishWords.Query().AsNoTracking()
                        .Where(w => w.Id != targetId)
                        .Select(w => w.Text)
                        .Distinct()
                        .Take(400)
                        .ToListAsync(ct);
                    seedEnglishPool.AddRange(globalPool);
                }

                var options = MakeOptions(correct, seedEnglishPool, 3);
                return new QuizQuestionDto { EnglishWordId = targetId, Prompt = primaryTr, Options = options };
            }
        }

        private List<string> MakeOptions(string correct, List<string> candidatePool, int distractorCount)
        {
            var opts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { correct };

            foreach (var s in Shuffle(candidatePool))
            {
                if (opts.Count >= distractorCount + 1) break;
                if (string.IsNullOrWhiteSpace(s)) continue;
                opts.Add(s);
            }
            while (opts.Count < distractorCount + 1) opts.Add("—");

            var list = opts.ToList();
            return Shuffle(list).ToList();
        }

        private IEnumerable<T> Shuffle<T>(IEnumerable<T> src)
        {
            var arr = src.ToList();
            for (int i = arr.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1); // <- _rng yerine Random.Shared
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr;
        }
        private static void ShuffleInPlace<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        public async Task<SubmitAnswerResult> SubmitAnswerAsync(SubmitAnswerRequest request, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(request.QuizRunId, ct)
                      ?? throw new InvalidOperationException("Quiz run not found.");

            var en = await _uow.EnglishWords.GetByIdAsync(request.EnglishWordId, ct)
                     ?? throw new InvalidOperationException("English word not found.");

            // Doğru cevap metnini hesapla
            string correctAnswer;
            if (run.Mode == QuizMode.EnglishToTurkish)
            {
                correctAnswer = await GetPrimaryTurkishTextAsync(en.Id, ct)
                                ?? throw new InvalidOperationException("This word has no Turkish translation.");
            }
            else // TurkishToEnglish
            {
                correctAnswer = en.Text;
            }

            var isCorrect = string.Equals(request.SelectedText?.Trim(), correctAnswer, StringComparison.OrdinalIgnoreCase);

            // Attempt + WordStat güncelle
            var attempt = new QuizAttempt
            {
                QuizRunId = run.Id,
                EnglishWordId = en.Id,
                IsCorrect = isCorrect
            };
            await _uow.QuizAttempts.AddAsync(attempt, ct);

            // Stat (liste bazlı)
            var stat = await _uow.WordStats.FirstOrDefaultAsync(x =>
                x.WordListId == run.WordListId && x.EnglishWordId == en.Id, ct);

            if (stat == null)
            {
                stat = new WordStat
                {
                    WordListId = run.WordListId,
                    EnglishWordId = en.Id,
                    TimesShown = 0,
                    CorrectCount = 0,
                    WrongCount = 0
                };
                await _uow.WordStats.AddAsync(stat, ct);
            }

            stat.TimesShown += 1;
            if (isCorrect) stat.CorrectCount += 1;
            else stat.WrongCount += 1;
            stat.LastShownAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync(ct);

            return new SubmitAnswerResult
            {
                IsCorrect = isCorrect,
                CorrectAnswer = correctAnswer
            };
        }
        public async Task<QuizSummaryDto> FinishAsync(int quizRunId, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct) ?? throw new InvalidOperationException("Quiz run not found.");
            run.FinishedAt = DateTime.UtcNow;

            var summary = await _uow.QuizAttempts.GetRunSummaryAsync(quizRunId, ct);

            var items = new List<QuizWordSummaryItem>();
            int totalCorrect = 0, totalWrong = 0, totalTimes = 0;

            var wordIds = summary.Select(s => s.EnglishWordId).ToList();
            var words = await _uow.EnglishWords.GetByIdsAsync(wordIds, ct);
            var wordMap = words.ToDictionary(w => w.Id, w => w.Text);

            foreach (var s in summary)
            {
                items.Add(new QuizWordSummaryItem
                {
                    EnglishWordId = s.EnglishWordId,
                    EnglishText = wordMap.TryGetValue(s.EnglishWordId, out var t) ? t : $"#{s.EnglishWordId}",
                    TimesShown = s.Times,
                    Correct = s.Correct,
                    Wrong = s.Wrong
                });
                totalCorrect += s.Correct;
                totalWrong += s.Wrong;
                totalTimes += s.Times;
            }

            await _uow.SaveChangesAsync(ct);

            return new QuizSummaryDto
            {
                QuizRunId = quizRunId,
                TotalShown = totalTimes,
                CorrectCount = totalCorrect,
                WrongCount = totalWrong,
                Items = items.OrderByDescending(x => x.Wrong).ThenByDescending(x => x.TimesShown).ToList()
            };
        }

        private async Task<string?> GetPrimaryTurkishTextAsync(int englishWordId, CancellationToken ct)
        {
            var primary = await _uow.WordTranslations.GetPrimaryAsync(englishWordId, ct);
            if (primary != null) return primary.TurkishWord.Text;

            var any = await _uow.WordTranslations.GetByEnglishAsync(englishWordId, ct);
            return any.FirstOrDefault()?.TurkishWord.Text;
        }

        public async Task ChangeModeAsync(int quizRunId, QuizMode newMode, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct)
                      ?? throw new InvalidOperationException("Quiz run not found.");
            run.Mode = newMode;
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<QuizMode> GetRunModeAsync(int quizRunId, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct)
                      ?? throw new InvalidOperationException("Quiz run not found.");
            return run.Mode;
        }
        
        public async Task<WordHintDto> GetHintsAsync(int quizRunId, int englishWordId, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct)
                      ?? throw new InvalidOperationException("Quiz run not found.");

            // Eş anlamlar (iki yön)
            var synFrom = await _uow.EnglishWordRelations.Query()
                .Where(r => r.EnglishWordId == englishWordId)
                .Include(r => r.RelatedEnglishWord)
                .Select(r => r.RelatedEnglishWord.Text)
                .ToListAsync(ct);

            var synTo = await _uow.EnglishWordRelations.Query()
                .Where(r => r.RelatedEnglishWordId == englishWordId)
                .Include(r => r.EnglishWord)
                .Select(r => r.EnglishWord.Text)
                .ToListAsync(ct);

            var synonyms = synFrom.Concat(synTo)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();

            var dto = new WordHintDto { Synonyms = synonyms };

            if (run.Mode == QuizMode.EnglishToTurkish)
            {
                // TR anlamlar
                dto.Meanings = await _uow.WordTranslations.Query()
                    .Where(t => t.EnglishWordId == englishWordId)
                    .Include(t => t.TurkishWord)
                    .OrderByDescending(t => t.IsPrimary).ThenBy(t => t.Id)
                    .Select(t => t.TurkishWord.Text)
                    .ToListAsync(ct);
            }
            else // TurkishToEnglish
            {
                // EN karşılık (cevap kelimenin kendisi)
                var en = await _uow.EnglishWords.GetByIdAsync(englishWordId, ct)
                         ?? throw new InvalidOperationException("Word not found.");
                dto.Meanings = new List<string> { en.Text }; // istersen lemma/variant ekleyebilirsin
            }

            return dto;
        }

        
            private async Task<List<int>> SelectSeedEnglishWordIdsAsync(int wordListId, int seedCount, CancellationToken ct = default)
        {
            seedCount = Math.Max(1, seedCount);

            // 1) Listenin kelimeleri (Id, Text)
            var words = await (
                from i in _uow.WordListItems.Query().AsNoTracking()
                where i.WordListId == wordListId
                join w in _uow.EnglishWords.Query().AsNoTracking()
                    on i.EnglishWordId equals w.Id
                select new { w.Id, w.Text }
            )
            .Distinct()
            .ToListAsync(ct);

            if (words.Count == 0) return new List<int>();
            var wordIds = words.Select(x => x.Id).ToList();

            // 2) WordStats (global sayaçlar) — silmelerden etkilenmez
            var stats = await _uow.WordStats.Query().AsNoTracking()
                .Where(s => wordIds.Contains(s.EnglishWordId))
                .Select(s => new { s.EnglishWordId, s.TimesShown })
                .ToListAsync(ct);
            var map = stats.ToDictionary(x => x.EnglishWordId, x => x.TimesShown);

            // 3) En az gösterilenden başla, eşitlikte alfabetik
            return words
                .OrderBy(w => map.TryGetValue(w.Id, out var c) ? c : 0)
                .ThenBy(w => w.Text, StringComparer.CurrentCultureIgnoreCase)
                .Take(seedCount)
                .Select(w => w.Id)
                .ToList();
        }


        public async Task<StartQuizResult> StartFromSeedAsync(StartFromSeedRequest req, CancellationToken ct = default)
        {
            if (req.SeedIds == null || req.SeedIds.Count == 0)
                throw new InvalidOperationException("Seed boş olamaz.");

            var cycle = req.SeedIds.ToList();
            ShuffleInPlace(cycle);

            var run = new QuizRun
            {
                WordListId = req.WordListId,
                Mode = req.Mode,
                SeedCount = req.SeedIds.Count,
                SeedWordIdsCsv = string.Join(",", req.SeedIds),
                CycleOrderCsv = string.Join(",", cycle),
                CycleIndex = 0,
                IsPractice = req.IsPractice,
                SourceRunId = req.SourceRunId,
                StartedAt = DateTime.UtcNow
            };

            await _uow.QuizRuns.AddAsync(run, ct);
            await _uow.SaveChangesAsync(ct);

            return new StartQuizResult { QuizRunId = run.Id, WordListId = run.WordListId, Mode = run.Mode };
        }

        public async Task<List<QuizRunSummaryDto>> GetRecentRunsAsync(int take = 20, CancellationToken ct = default)
        {
            var runs = await _uow.QuizRuns.Query().AsNoTracking()
                .OrderByDescending(r => r.StartedAt).Take(take).ToListAsync(ct);

            var listIds = runs.Select(r => r.WordListId).Distinct().ToList();
            var lists = await _uow.WordLists.Query().AsNoTracking()
                .Where(l => listIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Name })
                .ToListAsync(ct);
            var listMap = lists.ToDictionary(x => x.Id, x => x.Name);

            var runIds = runs.Select(r => r.Id).ToList();
            var stats = await _uow.QuizAttempts.Query().AsNoTracking()
                .Where(a => runIds.Contains(a.QuizRunId))
                .GroupBy(a => a.QuizRunId)
                .Select(g => new { RunId = g.Key, Total = g.Count(), Correct = g.Count(x => x.IsCorrect), Wrong = g.Count(x => !x.IsCorrect) })
                .ToListAsync(ct);
            var statMap = stats.ToDictionary(x => x.RunId, x => x);

            return runs.Select(r => new QuizRunSummaryDto
            {
                QuizRunId = r.Id,
                ListName = listMap.TryGetValue(r.WordListId, out var n) ? n : $"List {r.WordListId}",
                StartedAt = r.StartedAt,
                FinishedAt = r.FinishedAt,
                IsPractice = r.IsPractice,
                Mode = r.Mode,
                SeedCount = r.SeedCount,
                TotalShown = statMap.TryGetValue(r.Id, out var s) ? s.Total : 0,
                Correct = statMap.TryGetValue(r.Id, out var s1) ? s1.Correct : 0,
                Wrong = statMap.TryGetValue(r.Id, out var s2) ? s2.Wrong : 0
            }).ToList();
        }

        public async Task<QuizRunDetailDto> GetRunDetailAsync(int quizRunId, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct)
                      ?? throw new InvalidOperationException("Koşu bulunamadı.");
            var list = await _uow.WordLists.GetByIdAsync(run.WordListId, ct);

            var seed = (run.SeedWordIdsCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0).Where(id => id > 0).ToList();

            var atts = await _uow.QuizAttempts.Query().AsNoTracking()
                .Where(a => a.QuizRunId == quizRunId)
                .GroupBy(a => a.EnglishWordId)
                .Select(g => new { Id = g.Key, Shown = g.Count(), Correct = g.Count(x => x.IsCorrect), Wrong = g.Count(x => !x.IsCorrect) })
                .ToListAsync(ct);

            var words = await _uow.EnglishWords.Query().AsNoTracking()
                .Where(w => seed.Contains(w.Id))
                .Select(w => new { w.Id, w.Text })
                .ToListAsync(ct);
            var textMap = words.ToDictionary(x => x.Id, x => x.Text);

            var rows = seed.Select(id =>
            {
                var s = atts.FirstOrDefault(x => x.Id == id);
                return new QuizRunDetailDto.WordAttemptRow
                {
                    EnglishWordId = id,
                    English = textMap.TryGetValue(id, out var t) ? t : $"#{id}",
                    Shown = s?.Shown ?? 0,
                    Correct = s?.Correct ?? 0,
                    Wrong = s?.Wrong ?? 0
                };
            }).ToList();

            return new QuizRunDetailDto
            {
                QuizRunId = run.Id,
                WordListId = run.WordListId,
                ListName = list?.Name ?? $"List {run.WordListId}",
                IsPractice = run.IsPractice,
                Mode = run.Mode,
                SeedIds = seed,
                Words = rows
            };
        }

        public async Task SetRunPracticeAsync(int quizRunId, bool isPractice, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct)
                      ?? throw new InvalidOperationException("Koşu bulunamadı.");
            run.IsPractice = isPractice;
            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeleteRunAsync(int quizRunId, CancellationToken ct = default)
        {
            var run = await _uow.QuizRuns.GetByIdAsync(quizRunId, ct)
                      ?? throw new InvalidOperationException("Koşu bulunamadı.");
            // Attempts cascade ile silinir; WordStats'a dokunmuyoruz
            _uow.QuizRuns.Remove(run);
            await _uow.SaveChangesAsync(ct);
        }

        private static TimeZoneInfo ResolveTz(string tz)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(tz); }
            catch
            { // Windows vs Linux uyumu
                try { return TZConvert.GetTimeZoneInfo(tz); } catch { return TimeZoneInfo.Utc; }
            }
        }

        public async Task<Dictionary<DateOnly, int>> GetDailyCountsForMonthAsync(int year, int month, string timeZoneId, CancellationToken ct = default)
        {
            var tz = ResolveTz(timeZoneId);
            var firstLocal = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var nextMonthLocal = firstLocal.AddMonths(1);

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(firstLocal, tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(nextMonthLocal, tz);

            // Ay içindeki tüm run'lar
            var runs = await _uow.QuizRuns.Query().AsNoTracking()
                .Where(r => r.StartedAt >= startUtc && r.StartedAt < endUtc)
                .Select(r => new { r.Id, r.StartedAt })
                .ToListAsync(ct);

            // Günü yerel tarihe çevirip grupla
            var dict = new Dictionary<DateOnly, int>();
            foreach (var r in runs)
            {
                var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(r.StartedAt, DateTimeKind.Utc), tz);
                var d = DateOnly.FromDateTime(local.Date);
                dict[d] = dict.TryGetValue(d, out var c) ? c + 1 : 1;
            }
            return dict;
        }

        public async Task<List<QuizRunSummaryDto>> GetRunsForDayAsync(DateOnly day, string timeZoneId, CancellationToken ct = default)
        {
            var tz = ResolveTz(timeZoneId);
            var dayStartLocal = day.ToDateTime(TimeOnly.MinValue); // 00:00
            var dayEndLocal = day.ToDateTime(TimeOnly.MaxValue); // 23:59:59.9999999

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(dayEndLocal, tz);

            var runs = await _uow.QuizRuns.Query().AsNoTracking()
                .Where(r => r.StartedAt >= startUtc && r.StartedAt <= endUtc)
                .OrderBy(r => r.StartedAt)
                .ToListAsync(ct);

            var listIds = runs.Select(r => r.WordListId).Distinct().ToList();
            var lists = await _uow.WordLists.Query().AsNoTracking()
                .Where(l => listIds.Contains(l.Id))
                .Select(l => new { l.Id, l.Name })
                .ToListAsync(ct);
            var listMap = lists.ToDictionary(x => x.Id, x => x.Name);

            var runIds = runs.Select(r => r.Id).ToList();
            var stats = await _uow.QuizAttempts.Query().AsNoTracking()
                .Where(a => runIds.Contains(a.QuizRunId))
                .GroupBy(a => a.QuizRunId)
                .Select(g => new { RunId = g.Key, Total = g.Count(), Correct = g.Count(x => x.IsCorrect), Wrong = g.Count(x => !x.IsCorrect) })
                .ToListAsync(ct);
            var statMap = stats.ToDictionary(x => x.RunId, x => x);

            return runs.Select(r => new QuizRunSummaryDto
            {
                QuizRunId = r.Id,
                ListName = listMap.TryGetValue(r.WordListId, out var n) ? n : $"List {r.WordListId}",
                StartedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(r.StartedAt, DateTimeKind.Utc), tz),
                FinishedAt = r.FinishedAt.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(r.FinishedAt.Value, DateTimeKind.Utc), tz) : null,
                IsPractice = r.IsPractice,
                Mode = r.Mode,
                SeedCount = r.SeedCount,
                TotalShown = statMap.TryGetValue(r.Id, out var s) ? s.Total : 0,
                Correct = statMap.TryGetValue(r.Id, out var s1) ? s1.Correct : 0,
                Wrong = statMap.TryGetValue(r.Id, out var s2) ? s2.Wrong : 0
            }).ToList();
        }

    }
}
