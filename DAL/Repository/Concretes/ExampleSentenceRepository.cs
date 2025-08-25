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
    public class ExampleSentenceRepository : GenericRepository<ExampleSentence>, IExampleSentenceRepository
    {
        public ExampleSentenceRepository(AppDbContext ctx) : base(ctx) { }

        public Task<List<ExampleSentence>> GetByEnglishWordAsync(int englishWordId, CancellationToken ct = default)
            => _set.Where(x => x.EnglishWordId == englishWordId)
                   .OrderBy(x => x.OrderIndex)
                   .ToListAsync(ct);
    }
}
