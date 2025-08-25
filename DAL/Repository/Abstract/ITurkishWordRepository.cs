using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface ITurkishWordRepository : IGenericRepository<TurkishWord>
    {
        Task<TurkishWord?> GetByNormalizedAsync(string textNormalized, CancellationToken ct = default);
        Task<List<TurkishWord>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
