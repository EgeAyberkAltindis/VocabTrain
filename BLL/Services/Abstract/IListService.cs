using BLL.DTO;
using BLL.DTO.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BLL.DTO.ListDtos;

namespace BLL.Services.Abstract
{
    public interface IListService
    {
        Task<EnsureListResultDto> EnsureListAsync(string listName, CancellationToken ct = default);
        Task<bool> AddWordToListAsync(int englishWordId, string listName, CancellationToken ct = default);

        Task<List<ListWithCountDto>> GetAllWithCountsAsync(CancellationToken ct = default);
        Task<(string ListName, List<WordExportDto> Items)> GetForExportAsync(int wordListId, CancellationToken ct = default);
    }
}
