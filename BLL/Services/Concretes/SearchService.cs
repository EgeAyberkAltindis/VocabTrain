using BLL.DTO;
using BLL.Services.Abstract;
using DAL.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Concretes
{
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _uow;
        public SearchService(IUnitOfWork uow) { _uow = uow; }

        public async Task<List<WordSearchResultDto>> SearchAsync(string? query, int? listId, int take = 50, CancellationToken ct = default)
        {
            query = (query ?? string.Empty).Trim();
            var like = $"%{query}%";

            // Base: EnglishWords (opsiyonel: liste filtresi)
            var baseQ = _uow.EnglishWords.Query().AsNoTracking();

            if (listId.HasValue)
            {
                var idsInList = _uow.WordListItems.Query()
                                  .Where(i => i.WordListId == listId.Value)
                                  .Select(i => i.EnglishWordId);

                baseQ = baseQ.Where(w => idsInList.Contains(w.Id));
            }

            // Filtre: EN text veya TR çevirilerde geçenler
            if (!string.IsNullOrWhiteSpace(query))
            {
                baseQ = baseQ.Where(w =>
                    EF.Functions.Like(w.Text, like) ||
                    w.Translations.Any(t => EF.Functions.Like(t.TurkishWord.Text, like))
                );
            }

            // İlk aşamada sadece id'leri al (top N)
            var pickedIds = await baseQ
                .OrderBy(w => w.Text)
                .Select(w => w.Id)
                .Take(take)
                .ToListAsync(ct);

            if (pickedIds.Count == 0) return new List<WordSearchResultDto>();

            // Detaylar: TR anlamlar, cümleler, eş anlamlar
            var detailed = await _uow.EnglishWords.Query().AsNoTracking()
                .Where(w => pickedIds.Contains(w.Id))
                .Include(w => w.Translations).ThenInclude(t => t.TurkishWord)
                .Include(w => w.Sentences)
                .Include(w => w.RelationsFrom).ThenInclude(r => r.RelatedEnglishWord)
                .Include(w => w.RelationsTo).ThenInclude(r => r.EnglishWord)
                .ToListAsync(ct);

            // Map → DTO
            var results = detailed
                .OrderBy(w => w.Text) // görüntü sırası
                .Select(w =>
                {
                    var tr = w.Translations
                              .OrderByDescending(t => t.IsPrimary)
                              .ThenBy(t => t.Id)
                              .Select(t => t.TurkishWord.Text)
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList();

                    var syn = w.RelationsFrom.Select(r => r.RelatedEnglishWord.Text)
                               .Concat(w.RelationsTo.Select(r => r.EnglishWord.Text))
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .OrderBy(x => x)
                               .ToList();

                    var sent = w.Sentences
                                .OrderBy(s => s.OrderIndex)
                                .ThenBy(s => s.Id)
                                .Select(s => s.EnglishText)
                                .Take(2)
                                .ToList();

                    return new WordSearchResultDto
                    {
                        EnglishWordId = w.Id,
                        EnglishText = w.Text,
                        TurkishMeanings = tr,
                        Synonyms = syn,
                        ExampleSentences = sent
                    };
                })
                .ToList();

            return results;
        }
    }
}
