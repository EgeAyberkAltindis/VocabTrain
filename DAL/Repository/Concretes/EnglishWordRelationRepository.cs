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
    public class EnglishWordRelationRepository : GenericRepository<EnglishWordRelation>, IEnglishWordRelationRepository
    {
        public EnglishWordRelationRepository(AppDbContext ctx) : base(ctx) { }

        public Task<bool> RelationExistsAsync(int englishWordId, int relatedEnglishWordId, CancellationToken ct = default)
        {
            var a = Math.Min(englishWordId, relatedEnglishWordId);
            var b = Math.Max(englishWordId, relatedEnglishWordId);
            return _set.AnyAsync(x => x.EnglishWordId == a && x.RelatedEnglishWordId == b, ct);
        }

        public Task<List<EnglishWordRelation>> GetRelationsAsync(int englishWordId, CancellationToken ct = default)
        {
            return _set
                .Include(x => x.EnglishWord)
                .Include(x => x.RelatedEnglishWord)
                .Where(x => x.EnglishWordId == englishWordId || x.RelatedEnglishWordId == englishWordId)
                .ToListAsync(ct);
        }

        public async Task<List<int>> GetRelatedIdsAsync(int englishWordId, CancellationToken ct = default)
        {
            var forward = _set.Where(x => x.EnglishWordId == englishWordId)
                              .Select(x => x.RelatedEnglishWordId);
            var backward = _set.Where(x => x.RelatedEnglishWordId == englishWordId)
                               .Select(x => x.EnglishWordId);

            return await forward.Concat(backward).Distinct().ToListAsync(ct);
        }
    }
}
