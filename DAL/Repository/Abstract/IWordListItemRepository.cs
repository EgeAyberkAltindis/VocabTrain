using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IWordListItemRepository : IGenericRepository<WordListItem>
    {
        Task<bool> ExistsAsync(int wordListId, int englishWordId, CancellationToken ct = default);
        Task<List<WordListItem>> GetByListAsync(int wordListId, CancellationToken ct = default);
    }
}
