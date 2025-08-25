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
    public class QuizRunRepository : GenericRepository<QuizRun>, IQuizRunRepository
    {
        public QuizRunRepository(AppDbContext ctx) : base(ctx) { }

        public Task<QuizRun?> GetWithListAsync(int id, CancellationToken ct = default)
            => _set.Include(x => x.WordList)
                   .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
