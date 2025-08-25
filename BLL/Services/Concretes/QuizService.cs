using BLL.DTO;

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

namespace BLL.Services.Concretes
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _uow;
        private readonly Random _rng = new Random();

        public QuizService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<StartQuizResult> StartAsync(StartQuizRequest req, CancellationToken ct = default)
        {
            if (req.SeedCount < 1) req.SeedCount = 10;

            // 1) Seed: önce hiç sorulmamışlar
            var seedIds = await SelectSeedEnglishWordIdsAsync(req.WordListId, req.SeedCount, ct);
            if (seedIds.Count == 0) throw new InvalidOperationException("Seçilen listede hiç kelime yok.");

            // 2) İlk turun sırasını rastgele belirle
            var cycleOrder = seedIds.ToList();
            ShuffleInPlace(cycleOrder);

            var run = new QuizRun
            {
                WordListId = req.WordListId,
                Mode = req.Mode,
                SeedCount = seedIds.Count,
                SeedWordIdsCsv = string.Join(",", seedIds),
                CycleOrderCsv = string.Join(",", cycleOrder),
                CycleIndex = 0,
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

            var seed = run.SeedWordIdsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
            if (seed.Count == 0) return null;

            var cycle = (run.CycleOrderCsv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            // Koruma: cycle yoksa seed'i karıştır ve başlat
            if (cycle.Count != seed.Count)
            {
                cycle = seed.ToList();
                ShuffleInPlace(cycle);
                run.CycleOrderCsv = string.Join(",", cycle);
                run.CycleIndex = 0;
                await _uow.SaveChangesAsync(ct);
            }

            // Tüm seed bir kez sorulduysa → yeni tur için farklı bir shuffle üret
            if (run.CycleIndex >= cycle.Count)
            {
                var newOrder = seed.ToList();
                int safety = 0;
                do { ShuffleInPlace(newOrder); safety++; }
                while (safety < 5 && Enumerable.SequenceEqual(newOrder, cycle)); // öncekiyle aynı olmasın

                cycle = newOrder;
                run.CycleOrderCsv = string.Join(",", cycle);
                run.CycleIndex = 0;
                await _uow.SaveChangesAsync(ct);
            }

            // Sıradaki hedef
            var targetId = cycle[run.CycleIndex];

            // İlerle ve kaydet
            run.CycleIndex++;
            await _uow.SaveChangesAsync(ct);

            // Soruyu üret (çeldiriciler mümkünse seed'den)
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
        public async Task<WordHintDto> GetHintsAsync(int englishWordId, CancellationToken ct = default)
        {
            // TR anlamlar (primary önce)
            var trList = await _uow.WordTranslations.Query()
                .Where(t => t.EnglishWordId == englishWordId)
                .Include(t => t.TurkishWord)
                .OrderByDescending(t => t.IsPrimary)
                .ThenBy(t => t.Id)
                .Select(t => t.TurkishWord.Text)
                .ToListAsync(ct);

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

            var dto = new WordHintDto
            {
                Meanings = trList
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(System.StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Synonyms = synFrom.Concat(synTo)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(System.StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList()
            };

            return dto;
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

            // 1) Bu listenin tüm İngilizce kelimeleri (Id + Text) — tek, basit sorgu
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

            // 2) Bu listeye ait geçmiş denemelerde söz konusu kelimelerin toplam kaç kez sorulduğu
            var counts = await (
                from a in _uow.QuizAttempts.Query().AsNoTracking()
                join r in _uow.QuizRuns.Query().AsNoTracking()
                    on a.QuizRunId equals r.Id
                where r.WordListId == wordListId
                   && wordIds.Contains(a.EnglishWordId)        // yalnız bu listenin kelimeleri
                group a by a.EnglishWordId into g
                select new { Id = g.Key, Shown = g.Count() }
            )
            .ToListAsync(ct);

            var countMap = counts.ToDictionary(x => x.Id, x => x.Shown);

            // 3) Sıralama: önce en az sorulan, eşitlikte alfabetik (deterministik)
            var ordered = words
                .OrderBy(w => countMap.TryGetValue(w.Id, out var c) ? c : 0)
                .ThenBy(w => w.Text, StringComparer.CurrentCultureIgnoreCase)
                .Take(seedCount)
                .Select(w => w.Id)
                .ToList();

            return ordered;
        }
    }
}
