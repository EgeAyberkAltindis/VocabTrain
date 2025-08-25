
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



    }
}

