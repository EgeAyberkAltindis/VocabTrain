using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IWordTranslationRepository : IGenericRepository<WordTranslation>
    {
        Task<bool> ExistsAsync(int englishWordId, int turkishWordId, CancellationToken ct = default);
        Task<List<WordTranslation>> GetByEnglishAsync(int englishWordId, CancellationToken ct = default);
        Task<List<WordTranslation>> GetByTurkishAsync(int turkishWordId, CancellationToken ct = default);
        Task<WordTranslation?> GetPrimaryAsync(int englishWordId, CancellationToken ct = default);
    }
}
