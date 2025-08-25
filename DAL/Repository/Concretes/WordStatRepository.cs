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
    public class WordStatRepository : GenericRepository<WordStat>, IWordStatRepository
    {
        public WordStatRepository(AppDbContext ctx) : base(ctx) { }

        public Task<WordStat?> GetAsync(int wordListId, int englishWordId, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(x => x.WordListId == wordListId && x.EnglishWordId == englishWordId, ct);

        public Task<List<WordStat>> GetByListAsync(int wordListId, CancellationToken ct = default)
            => _set.Where(x => x.WordListId == wordListId).ToListAsync(ct);

        // En az gösterilenler (eşitlikte EnglishWordId artan; BLL randomize edebilir)
        public Task<List<int>> GetLeastSeenWordIdsAsync(int wordListId, int take, CancellationToken ct = default)
            => _set.Where(x => x.WordListId == wordListId)
                   .OrderBy(x => x.TimesShown)
                   .ThenBy(x => x.EnglishWordId)
                   .Select(x => x.EnglishWordId)
                   .Take(take)
                   .ToListAsync(ct);

        // En çok yanlış oranı (WrongRate = Wrong / (Correct+Wrong)), sonra TimesShown desc
        public Task<List<int>> GetMostWrongWordIdsAsync(int wordListId, int take, CancellationToken ct = default)
            => _set.Where(x => x.WordListId == wordListId && (x.CorrectCount + x.WrongCount) > 0)
                   .OrderByDescending(x => (double)x.WrongCount / (x.CorrectCount + x.WrongCount))
                   .ThenByDescending(x => x.TimesShown)
                   .Select(x => x.EnglishWordId)
                   .Take(take)
                   .ToListAsync(ct);
    }
}
