using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IWordStatRepository : IGenericRepository<WordStat>
    {
        Task<WordStat?> GetAsync(int wordListId, int englishWordId, CancellationToken ct = default);
        Task<List<WordStat>> GetByListAsync(int wordListId, CancellationToken ct = default);

        // Seçim stratejileri için yardımcı sorgular:
        Task<List<int>> GetLeastSeenWordIdsAsync(int wordListId, int take, CancellationToken ct = default);
        Task<List<int>> GetMostWrongWordIdsAsync(int wordListId, int take, CancellationToken ct = default);   // WrongRate ile
    }
}
