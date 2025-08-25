using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IQuizAttemptRepository : IGenericRepository<QuizAttempt>
    {
        Task<List<(int EnglishWordId, int Correct, int Wrong, int Times)>> GetRunSummaryAsync(int quizRunId, CancellationToken ct = default);
    }
}
