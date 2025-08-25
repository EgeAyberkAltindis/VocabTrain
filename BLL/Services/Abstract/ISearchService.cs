using BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Abstract
{
    public interface ISearchService
    {
        /// <summary>
        /// İngilizce/Türkçe metinde arar. listId verilirse o listenin içinde.
        /// query boşsa ilk N (alfabetik) döner.
        /// </summary>
        Task<List<WordSearchResultDto>> SearchAsync(string? query, int? listId, int take = 50, CancellationToken ct = default);
    }
}
