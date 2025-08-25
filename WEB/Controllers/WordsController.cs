using BLL.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using WEB.Models.ViewModels;
using WEB.Utils;

namespace WEB.Controllers
{
    public class WordsController : Controller
    {
       

        private readonly IWordService _wordService;

        public WordsController(IWordService wordService)
        {
            _wordService = wordService;
        }

        [HttpGet]
        public IActionResult Import() => View(new WordImportViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(WordImportViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var dto = WordInputParser.ParseSingle(vm.Raw!, vm.TargetListName);
                var res = await _wordService.UpsertSingleAsync(dto, ct);
                vm.Message = $"[EN:{res.EnglishText}] işlendi. TR+: {res.AddedTurkishCount}, Çev+: {res.AddedTranslationsCount}, Cümle+: {res.AddedSentencesCount}, EşAnlam+: {res.AddedSynonymRelationsCount}, Liste+: {res.AddedToList}";
                ModelState.Clear();
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }


        [HttpGet]
        public IActionResult ImportBulk() => View(new BulkWordImportViewModel());
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportBulk(BulkWordImportViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var dtos = WordInputParser.ParseBulk(vm.Raw!, vm.TargetListName);
                var results = await _wordService.UpsertBulkAsync(dtos, ct);
                vm.Message = $"Toplam {results.Count} kelime işlendi.";
                ModelState.Clear();
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }


    }
}

