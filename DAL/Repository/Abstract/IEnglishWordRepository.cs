using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IEnglishWordRepository : IGenericRepository<EnglishWord>
    {
        Task<EnglishWord?> GetByNormalizedAsync(string textNormalized, CancellationToken ct = default);
        Task<EnglishWord?> GetWithDetailsAsync(int id, CancellationToken ct = default); // Translations + Sentences dahil
        Task<List<EnglishWord>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
