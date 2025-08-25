using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IWordListRepository : IGenericRepository<WordList>
    {
        Task<WordList?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<List<int>> GetEnglishWordIdsAsync(int wordListId, CancellationToken ct = default);
    }
}
