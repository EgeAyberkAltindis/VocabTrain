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
    public class EnglishWordRepository : GenericRepository<EnglishWord>, IEnglishWordRepository
    {
        public EnglishWordRepository(AppDbContext ctx) : base(ctx) { }

        public Task<EnglishWord?> GetByNormalizedAsync(string textNormalized, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(x => x.TextNormalized == textNormalized, ct);

        public Task<List<EnglishWord>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
            => _set.Where(w => ids.Contains(w.Id)).ToListAsync(ct);

        public Task<EnglishWord?> GetWithDetailsAsync(int id, CancellationToken ct = default)
            => _set
                .Include(w => w.Translations).ThenInclude(t => t.TurkishWord)
                .Include(w => w.Sentences)
                .FirstOrDefaultAsync(w => w.Id == id, ct);
    }
}
