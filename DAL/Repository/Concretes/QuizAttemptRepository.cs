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
    public class QuizAttemptRepository : GenericRepository<QuizAttempt>, IQuizAttemptRepository
    {
        public QuizAttemptRepository(AppDbContext ctx) : base(ctx) { }

        // Oturum özeti: kelime bazında (Correct, Wrong, Times)
        public async Task<List<(int EnglishWordId, int Correct, int Wrong, int Times)>> GetRunSummaryAsync(int quizRunId, CancellationToken ct = default)
        {
            var data = await _set
                .Where(a => a.QuizRunId == quizRunId)
                .GroupBy(a => a.EnglishWordId)
                .Select(g => new
                {
                    EnglishWordId = g.Key,
                    Correct = g.Count(x => x.IsCorrect),
                    Wrong = g.Count(x => !x.IsCorrect),
                    Times = g.Count()
                })
                .ToListAsync(ct);

            return data.Select(x => (x.EnglishWordId, x.Correct, x.Wrong, x.Times)).ToList();
        }
    }
}
