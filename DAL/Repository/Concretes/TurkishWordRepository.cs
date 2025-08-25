using DAL.Context;
using DAL.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Concretes
{
    public class TurkishWordRepository : GenericRepository<TurkishWord>, ITurkishWordRepository
    {
        public TurkishWordRepository(AppDbContext ctx) : base(ctx) { }

        public Task<TurkishWord?> GetByNormalizedAsync(string textNormalized, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(x => x.TextNormalized == textNormalized, ct);

        public Task<List<TurkishWord>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
            => _set.Where(w => ids.Contains(w.Id)).ToListAsync(ct);
    }
}
