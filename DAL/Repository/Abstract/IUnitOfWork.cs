using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Abstract
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IEnglishWordRepository EnglishWords { get; }
        ITurkishWordRepository TurkishWords { get; }
        IWordTranslationRepository WordTranslations { get; }
        IExampleSentenceRepository ExampleSentences { get; }
        IEnglishWordRelationRepository EnglishWordRelations { get; }
        IWordListRepository WordLists { get; }
        IWordListItemRepository WordListItems { get; }
        IQuizRunRepository QuizRuns { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IWordStatRepository WordStats { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolation = IsolationLevel.ReadCommitted, CancellationToken ct = default);
    }
}
