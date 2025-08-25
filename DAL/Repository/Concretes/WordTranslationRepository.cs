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
    public class WordTranslationRepository : GenericRepository<WordTranslation>, IWordTranslationRepository
    {
        public WordTranslationRepository(AppDbContext ctx) : base(ctx) { }

        public Task<bool> ExistsAsync(int englishWordId, int turkishWordId, CancellationToken ct = default)
            => _set.AnyAsync(x => x.EnglishWordId == englishWordId && x.TurkishWordId == turkishWordId, ct);

        public Task<List<WordTranslation>> GetByEnglishAsync(int englishWordId, CancellationToken ct = default)
            => _set
                .Include(x => x.TurkishWord)
                .Where(x => x.EnglishWordId == englishWordId)
                .OrderByDescending(x => x.IsPrimary)
                .ToListAsync(ct);

        public Task<List<WordTranslation>> GetByTurkishAsync(int turkishWordId, CancellationToken ct = default)
            => _set
                .Include(x => x.EnglishWord)
                .Where(x => x.TurkishWordId == turkishWordId)
                .ToListAsync(ct);

        public Task<WordTranslation?> GetPrimaryAsync(int englishWordId, CancellationToken ct = default)
            => _set
                .Include(x => x.TurkishWord)
                .FirstOrDefaultAsync(x => x.EnglishWordId == englishWordId && x.IsPrimary, ct);
    }
}
