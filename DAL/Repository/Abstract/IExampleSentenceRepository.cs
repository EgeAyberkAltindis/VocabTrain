using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IExampleSentenceRepository : IGenericRepository<ExampleSentence>
    {
        Task<List<ExampleSentence>> GetByEnglishWordAsync(int englishWordId, CancellationToken ct = default);
    }
}
