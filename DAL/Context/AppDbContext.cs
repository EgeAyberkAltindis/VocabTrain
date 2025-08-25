using DAL.Configurations;
using Microsoft.EntityFrameworkCore;
using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
            
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {

        }

        public DbSet<EnglishWord> EnglishWords => Set<EnglishWord>();
        public DbSet<TurkishWord> TurkishWords => Set<TurkishWord>();
        public DbSet<WordTranslation> WordTranslations => Set<WordTranslation>();
        public DbSet<EnglishWordRelation> EnglishWordRelations => Set<EnglishWordRelation>();
        public DbSet<ExampleSentence> ExampleSentences => Set<ExampleSentence>();
        public DbSet<WordList> WordLists => Set<WordList>();
        public DbSet<WordListItem> WordListItems => Set<WordListItem>();
        public DbSet<QuizRun> QuizRuns => Set<QuizRun>();
        public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
        public DbSet<WordStat> WordStats => Set<WordStat>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlServer("server=DESKTOP-ABTB3OG\\SQLEXPRESS;Database=VocabTrainigDB;Trusted_Connection=True;TrustServerCertificate=true");
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new WordTranslationConfiguration());
            modelBuilder.ApplyConfiguration(new WordStatConfiguration());
            modelBuilder.ApplyConfiguration(new WordListItemConfiguration());
            modelBuilder.ApplyConfiguration(new WordListConfiguration());   
            modelBuilder.ApplyConfiguration(new TurkishWordConfiguration());
            modelBuilder.ApplyConfiguration(new QuizRunConfiguration());
            modelBuilder.ApplyConfiguration(new QuizAttemptConfiguration());
            modelBuilder.ApplyConfiguration(new ExampleSentenceConfiguration());
            modelBuilder.ApplyConfiguration(new EnglishWordRelationConfiguration());
            modelBuilder.ApplyConfiguration(new EnglishWordConfiguration());
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
