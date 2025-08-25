using BLL.DTO;
using BLL.DTO.BLL.DTOs;
using BLL.Services.Abstract;
using DAL.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BLL.DTO.ListDtos;

namespace BLL.Services.Concretes
{
    public class ListService : IListService
    {
        private readonly IUnitOfWork _uow;

        public ListService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<EnsureListResultDto> EnsureListAsync(string listName, CancellationToken ct = default)
        {
            var existing = await _uow.WordLists.GetByNameAsync(listName.Trim(), ct);
            if (existing != null)
            {
                return new EnsureListResultDto { WordListId = existing.Id, Name = existing.Name, IsCreated = false };
            }

            var wl = new WordList { Name = listName.Trim() };
            await _uow.WordLists.AddAsync(wl, ct);
            await _uow.SaveChangesAsync(ct);

            return new EnsureListResultDto { WordListId = wl.Id, Name = wl.Name, IsCreated = true };
        }

        public async Task<bool> AddWordToListAsync(int englishWordId, string listName, CancellationToken ct = default)
        {
            var res = await EnsureListAsync(listName, ct);
            if (await _uow.WordListItems.ExistsAsync(res.WordListId, englishWordId, ct))
                return false;

            await _uow.WordListItems.AddAsync(new WordListItem
            {
                WordListId = res.WordListId,
                EnglishWordId = englishWordId
            }, ct);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<ListWithCountDto>> GetAllWithCountsAsync(CancellationToken ct = default)
        {
            // WordLists left join WordListItems → Count
            var q =
                from wl in _uow.WordLists.Query()
                join wli in _uow.WordListItems.Query() on wl.Id equals wli.WordListId into g
                orderby wl.Name
                select new ListWithCountDto
                {
                    WordListId = wl.Id,
                    Name = wl.Name,
                    WordCount = g.Count()
                };

            return await System.Threading.Tasks.Task.FromResult(q.ToList());
        }


        public async Task<(string ListName, List<WordExportDto> Items)> GetForExportAsync(int wordListId, CancellationToken ct = default)
        {
            var list = await _uow.WordLists.GetByIdAsync(wordListId, ct)
                       ?? throw new InvalidOperationException("Liste bulunamadı.");

            // Bu listeye ait İngilizce kelimeleri tek sorguda detaylarıyla al
            var words = await _uow.EnglishWords.Query().AsNoTracking()
                .Where(w => _uow.WordListItems.Query().Any(i => i.WordListId == wordListId && i.EnglishWordId == w.Id))
                .Include(w => w.Translations).ThenInclude(t => t.TurkishWord)
                .Include(w => w.Sentences)
                .Include(w => w.RelationsFrom).ThenInclude(r => r.RelatedEnglishWord)
                .Include(w => w.RelationsTo).ThenInclude(r => r.EnglishWord)
                .AsSplitQuery()
                .OrderBy(w => w.Text)
                .ToListAsync(ct);

            var items = words.Select(w =>
            {
                var meanings = w.Translations
                    .OrderByDescending(t => t.IsPrimary).ThenBy(t => t.Id)
                    .Select(t => t.TurkishWord.Text)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var synonyms = w.RelationsFrom.Select(r => r.RelatedEnglishWord.Text)
                    .Concat(w.RelationsTo.Select(r => r.EnglishWord.Text))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList();

                var sentences = w.Sentences
                    .Select(s => s.EnglishText) // Not: sende alan adı farklıysa (Content vs.) burayı uyarlay
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                return new WordExportDto
                {
                    English = w.Text,
                    TurkishMeanings = meanings,
                    Synonyms = synonyms,
                    ExampleSentences = sentences
                };
            }).ToList();

            return (list.Name, items);
        }
    }
}

