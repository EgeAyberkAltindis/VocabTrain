using DAL.Repository.Abstract;
using DAL.Repository.Concretes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DependencyResolvers
{
    public static class DalDiExtensions
    {
        public static IServiceCollection AddDal(this IServiceCollection services)
        {
            services.AddScoped<IEnglishWordRepository, EnglishWordRepository>();
            services.AddScoped<ITurkishWordRepository, TurkishWordRepository>();
            services.AddScoped<IWordTranslationRepository, WordTranslationRepository>();
            services.AddScoped<IExampleSentenceRepository, ExampleSentenceRepository>();
            services.AddScoped<IEnglishWordRelationRepository, EnglishWordRelationRepository>();
            services.AddScoped<IWordListRepository, WordListRepository>();
            services.AddScoped<IWordListItemRepository, WordListItemRepository>();
            services.AddScoped<IQuizRunRepository, QuizRunRepository>();
            services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
            services.AddScoped<IWordStatRepository, WordStatRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}