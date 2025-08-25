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
    public class WordListRepository : GenericRepository<WordList>, IWordListRepository
    {
        public WordListRepository(AppDbContext ctx) : base(ctx) { }

        public Task<WordList?> GetByNameAsync(string name, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(x => x.Name == name, ct);

        public Task<List<int>> GetEnglishWordIdsAsync(int wordListId, CancellationToken ct = default)
            => _ctx.WordListItems
                   .Where(i => i.WordListId == wordListId)
                   .Select(i => i.EnglishWordId)
                   .ToListAsync(ct);
    }
}
