using BLL.Services.Abstract;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace WEB.Controllers
{


    public class BrowseController : Controller
    {
        private readonly IListService _listService;
        private readonly ISearchService _search;

        public BrowseController(IListService listService, ISearchService search)
        {
            _listService = listService;
            _search = search;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var lists = await _listService.GetAllWithCountsAsync(ct);
            return View(lists);
        }

        [HttpGet]
        public async Task<IActionResult> List(int id, CancellationToken ct)
        {
            var lists = await _listService.GetAllWithCountsAsync(ct);
            var current = lists.Find(x => x.WordListId == id);
            if (current == null) return RedirectToAction(nameof(Index));
            ViewBag.CurrentList = current;
            return View(lists);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? q, int? listId, int take = 50, CancellationToken ct = default)
        {
            var data = await _search.SearchAsync(q, listId, take, ct);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> Export(int id, CancellationToken ct)
        {
            var (listName, items) = await _listService.GetForExportAsync(id, ct);

            using var wb = new XLWorkbook();
            var sheetName = SanitizeSheetName(listName);
            var ws = wb.Worksheets.Add(sheetName);

            // Başlıklar
            ws.Cell(1, 1).Value = "English";
            ws.Cell(1, 2).Value = "Türkçe Anlamlar";
            ws.Cell(1, 3).Value = "Eş Anlamlılar";
            ws.Cell(1, 4).Value = "Örnek Cümleler";

            var header = ws.Range(1, 1, 1, 4); // 1. satır, 1-4 sütun
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Satırlar
            var r = 2;
            foreach (var x in items)
            {
                ws.Cell(r, 1).Value = x.English;
                ws.Cell(r, 2).Value = string.Join(", ", x.TurkishMeanings);
                ws.Cell(r, 3).Value = string.Join(", ", x.Synonyms);
                ws.Cell(r, 4).Value = string.Join("\n", x.ExampleSentences);
                r++;
            }

            ws.Column(4).Style.Alignment.WrapText = true;
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var fileName = SanitizeFileName($"{listName}.xlsx");
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Yardımcılar
        private static string SanitizeSheetName(string name)
        {
            // Excel sheet adı max 31 karakter ve şu karakterleri içeremez: : \ / ? * [ ]
            var n = Regex.Replace(name, @"[:\\/\?\*\[\]]", "_");
            return n.Length > 31 ? n.Substring(0, 31) : n;
        }
        private static string SanitizeFileName(string name)
        {
            var n = Regex.Replace(name, @"[^\w\-. ]+", "_", RegexOptions.CultureInvariant);
            return string.IsNullOrWhiteSpace(n) ? "export.xlsx" : n;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteList(int id, CancellationToken ct)
        {
            await _listService.DeleteAsync(id, ct);
            TempData["Toast"] = "Liste silindi.";
            return RedirectToAction(nameof(Index)); // tüm listelerin olduğu sayfa
        }

    }
}
    

