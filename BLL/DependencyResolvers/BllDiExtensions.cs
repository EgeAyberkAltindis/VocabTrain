using BLL.Services.Abstract;
using BLL.Services.Concretes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DependencyResolvers
{
    public static class BllDiExtensions
    {
        public static IServiceCollection AddBll(this IServiceCollection services)
        {
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IWordService, WordService>();
            services.AddScoped<IListService, ListService>();
            services.AddScoped<IQuizService, QuizService>();
            return services;
        }
    }
}
