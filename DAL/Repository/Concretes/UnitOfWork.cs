using DAL.Context;
using DAL.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository.Concretes
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _ctx;

        public UnitOfWork(AppDbContext ctx,
            IEnglishWordRepository englishWords,
            ITurkishWordRepository turkishWords,
            IWordTranslationRepository wordTranslations,
            IExampleSentenceRepository exampleSentences,
            IEnglishWordRelationRepository englishWordRelations,
            IWordListRepository wordLists,
            IWordListItemRepository wordListItems,
            IQuizRunRepository quizRuns,
            IQuizAttemptRepository quizAttempts,
            IWordStatRepository wordStats)
        {
            _ctx = ctx;
            EnglishWords = englishWords;
            TurkishWords = turkishWords;
            WordTranslations = wordTranslations;
            ExampleSentences = exampleSentences;
            EnglishWordRelations = englishWordRelations;
            WordLists = wordLists;
            WordListItems = wordListItems;
            QuizRuns = quizRuns;
            QuizAttempts = quizAttempts;
            WordStats = wordStats;
        }

        public IEnglishWordRepository EnglishWords { get; }
        public ITurkishWordRepository TurkishWords { get; }
        public IWordTranslationRepository WordTranslations { get; }
        public IExampleSentenceRepository ExampleSentences { get; }
        public IEnglishWordRelationRepository EnglishWordRelations { get; }
        public IWordListRepository WordLists { get; }
        public IWordListItemRepository WordListItems { get; }
        public IQuizRunRepository QuizRuns { get; }
        public IQuizAttemptRepository QuizAttempts { get; }
        public IWordStatRepository WordStats { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _ctx.SaveChangesAsync(ct);

        public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolation = IsolationLevel.ReadCommitted, CancellationToken ct = default)
            => _ctx.Database.BeginTransactionAsync(isolation, ct);

        public ValueTask DisposeAsync() => _ctx.DisposeAsync();
    }
}
