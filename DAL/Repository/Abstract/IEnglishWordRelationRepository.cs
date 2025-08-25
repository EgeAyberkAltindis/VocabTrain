using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IEnglishWordRelationRepository : IGenericRepository<EnglishWordRelation>
    {
        // İki kelime arasında ilişki var mı? (A–B veya B–A fark etmez)
        Task<bool> RelationExistsAsync(int englishWordId, int relatedEnglishWordId, CancellationToken ct = default);

        // Bir kelimenin tüm ilişkileri (iki yönlü)
        Task<List<EnglishWordRelation>> GetRelationsAsync(int englishWordId, CancellationToken ct = default);

        // Sadece id listesi istersen (performans için)
        Task<List<int>> GetRelatedIdsAsync(int englishWordId, CancellationToken ct = default);
    }
}
