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
    public class WordListItemRepository : GenericRepository<WordListItem>, IWordListItemRepository
    {
        public WordListItemRepository(AppDbContext ctx) : base(ctx) { }

        public Task<bool> ExistsAsync(int wordListId, int englishWordId, CancellationToken ct = default)
            => _set.AnyAsync(x => x.WordListId == wordListId && x.EnglishWordId == englishWordId, ct);

        public Task<List<WordListItem>> GetByListAsync(int wordListId, CancellationToken ct = default)
            => _set.Include(x => x.EnglishWord)
                   .Where(x => x.WordListId == wordListId)
                   .ToListAsync(ct);
    }
}
