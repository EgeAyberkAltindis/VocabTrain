using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IQuizRunRepository : IGenericRepository<QuizRun>
    {
        Task<QuizRun?> GetWithListAsync(int id, CancellationToken ct = default);
    }
}
