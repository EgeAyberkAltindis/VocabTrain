using BLL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Abstract
{
    public interface IWordService
    {
        Task<UpsertResultDto> UpsertSingleAsync(WordUpsertDto dto, CancellationToken ct = default);
        Task<List<UpsertResultDto>> UpsertBulkAsync(IEnumerable<WordUpsertDto> dtos, CancellationToken ct = default);
    }
}
