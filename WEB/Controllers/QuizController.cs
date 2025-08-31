
using BLL.DTO;
using BLL.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using MODEL.Entities;
using WEB.Models.ViewModels;
using static BLL.DTO.QuizDtos;

namespace WEB.Controllers
{
    public class QuizController : Controller
    {
        private readonly IListService _listService;
        private readonly IQuizService _quizService;
        private const string TzId = "Europe/Istanbul"; // projen için sabitleyebiliriz

        public QuizController(IListService listService, IQuizService quizService)
        {
            _listService = listService;
            _quizService = quizService;
        }

        [HttpGet]
        public async Task<IActionResult> Start(CancellationToken ct)
        {
            var lists = await _listService.GetAllWithCountsAsync(ct);
            return View(new StartQuizViewModel { Lists = lists });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(StartQuizViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid || vm.SelectedListId == null)
            {
                vm.Message = "Lütfen bir liste seçin.";
                vm.Lists = await _listService.GetAllWithCountsAsync(ct);
                return View(vm);
            }

            var req = new StartQuizRequest
            {
                WordListId = vm.SelectedListId.Value,
                Mode = vm.Mode,
                SeedCount = vm.SeedCount,
               
            };

            var result = await _quizService.StartAsync(req, ct);
            return RedirectToAction(nameof(Play), new { quizRunId = result.QuizRunId, wordListId = result.WordListId, mode = result.Mode });
        }


        [HttpGet]
        public async Task<IActionResult> Play(int quizRunId, int wordListId, MODEL.Entities.QuizMode? mode, CancellationToken ct)
        {
            // Parametre gelmediyse DB'den oku (gelmiş olsa da DB'yi kaynak kabul etmek iyi pratik)
            var currentMode = await _quizService.GetRunModeAsync(quizRunId, ct);
            var vm = new PlayQuizViewModel { QuizRunId = quizRunId, WordListId = wordListId, Mode = currentMode };
            return View(vm);
        }

        // ---- AJAX endpoint'leri (JSON döner) ----
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Next(int quizRunId, CancellationToken ct)
        {
            var q = await _quizService.GetNextQuestionAsync(quizRunId, ct);
            return Json(q);
        }

        public class AnswerRequest { public int QuizRunId { get; set; } public int EnglishWordId { get; set; } public string SelectedText { get; set; } = null!; }

        [HttpPost]
        [ValidateAntiForgeryToken] // << Artık token istiyoruz
        public async Task<IActionResult> Answer([FromBody] AnswerRequest req, CancellationToken ct)
        {
            var res = await _quizService.SubmitAnswerAsync(new BLL.DTO.QuizDtos . SubmitAnswerRequest
            {
                QuizRunId = req.QuizRunId,
                EnglishWordId = req.EnglishWordId,
                SelectedText = req.SelectedText
            }, ct);
            return Json(res);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(int quizRunId, CancellationToken ct)
        {
            var summary = await _quizService.FinishAsync(quizRunId, ct);
            return RedirectToAction(nameof(Summary), new { quizRunId = summary.QuizRunId });
        }

        [HttpGet]
        public async Task<IActionResult> Summary(int quizRunId, CancellationToken ct)
        {
            var summary = await _quizService.FinishAsync(quizRunId, ct);
            return View(new QuizSummaryViewModel { Summary = summary });
        }


        public class ChangeModeRequest { public int QuizRunId { get; set; } public QuizMode Mode { get; set; } }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeMode([FromBody] ChangeModeRequest req, CancellationToken ct)
        {
            await _quizService.ChangeModeAsync(req.QuizRunId, req.Mode, ct);
            return Ok(new { ok = true });
        }
        // ... class QuizController : Controller
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Hints(int quizRunId, int englishWordId, CancellationToken ct)
        {
            var hints = await _quizService.GetHintsAsync(quizRunId, englishWordId, ct);
            return Json(hints); // { meanings:[], synonyms:[] }
        }

        [HttpGet]
        public async Task<IActionResult> History(CancellationToken ct)
        {
            var runs = await _quizService.GetRecentRunsAsync(20, ct);
            return View(runs);
        }

        [HttpGet]
        public async Task<IActionResult> Run(int id, CancellationToken ct)
        {
            var vm = await _quizService.GetRunDetailAsync(id, ct);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Replay(int id, bool practice = false, CancellationToken ct = default)
        {
            var src = await _quizService.GetRunDetailAsync(id, ct);
            var res = await _quizService.StartFromSeedAsync(new StartFromSeedRequest
            {
                WordListId = src.WordListId,
                Mode = src.Mode,
                SeedIds = src.SeedIds,
                IsPractice = practice,
                SourceRunId = id
            }, ct);

            return RedirectToAction(nameof(Play), new { quizRunId = res.QuizRunId, wordListId = res.WordListId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPractice(int id, bool value, CancellationToken ct)
        {
            await _quizService.SetRunPracticeAsync(id, value, ct);
            return RedirectToAction(nameof(Run), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRun(int id, CancellationToken ct)
        {
            await _quizService.DeleteRunAsync(id, ct);
            TempData["Toast"] = "Quiz geçmişi silindi.";
            return RedirectToAction(nameof(History));
        }
        [HttpGet]
        public async Task<IActionResult> Calendar(int? year, int? month, DateTime? date, CancellationToken ct)
        {
            // Seçimler
            var nowLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"));
            var sel = date?.Date ?? nowLocal.Date;

            int y = year ?? sel.Year;
            int m = month ?? sel.Month;

            var selected = DateOnly.FromDateTime(sel);

            // Ayın günlük sayımları
            var counts = await _quizService.GetDailyCountsForMonthAsync(y, m, TzId, ct);

            // Takvim 6x7 grid (Pazartesi ilk sütun)
            var first = new DateTime(y, m, 1);
            int offset = ((int)first.DayOfWeek + 6) % 7; // Monday=0
            var gridStart = first.AddDays(-offset);

            var days = new List<CalendarViewModel.DayCell>(42);
            for (int i = 0; i < 42; i++)
            {
                var d = DateOnly.FromDateTime(gridStart.AddDays(i));
                counts.TryGetValue(d, out var c);
                var cell = new CalendarViewModel.DayCell
                {
                    Date = d,
                    InMonth = d.Month == m,
                    IsToday = d == DateOnly.FromDateTime(nowLocal.Date),
                    IsSelected = d == selected,
                    Count = c
                };
                // Özel kırmızı yuvarlak: 1/3/7/15 gün önce
                var ago = (DateOnly.FromDateTime(nowLocal.Date).DayNumber - d.DayNumber);
                var diff = DateOnly.FromDateTime(nowLocal.Date).ToDateTime(TimeOnly.MinValue) - d.ToDateTime(TimeOnly.MinValue);
                var daysAgo = (int)diff.TotalDays;
                if (new[] { 1, 3, 7, 15 }.Contains(daysAgo)) cell.SpecialAgo = daysAgo;

                days.Add(cell);
            }

            // Gün şeridi (seçili gün +-3)
            var strip = new List<DateOnly>();
            for (int i = -3; i <= 3; i++)
                strip.Add(selected.AddDays(i));

            // Seçili günün run'ları
            var runs = await _quizService.GetRunsForDayAsync(selected, TzId, ct);

            var vm = new CalendarViewModel
            {
                Year = y,
                Month = m,
                SelectedDay = selected,
                Days = days,
                Runs = runs,
                StripDays = strip
            };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ByDay(string day, CancellationToken ct)
        {
            if (!DateOnly.TryParse(day, out var d)) return BadRequest();
            var runs = await _quizService.GetRunsForDayAsync(d, TzId, ct);
            return PartialView("_RunsOfDay", runs);
        }
    }

}


