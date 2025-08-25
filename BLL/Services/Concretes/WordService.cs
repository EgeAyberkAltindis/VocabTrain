using BLL.DTO;
using BLL.Extension;
using BLL.Services.Abstract;
using DAL.Repository.Abstract;
using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Concretes
{
    public class WordService : IWordService
    {
        private readonly IUnitOfWork _uow;

        public WordService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<UpsertResultDto> UpsertSingleAsync(WordUpsertDto dto, CancellationToken ct = default)
        {
            // Transaction
            await using var tx = await _uow.BeginTransactionAsync(ct: ct);

            var result = new UpsertResultDto
            {
                EnglishText = dto.EnglishText,
                TargetListName = dto.TargetListName
            };

            // 1) EN kelime
            var enNorm = dto.EnglishText.NormalizeForLookup();
            var en = await _uow.EnglishWords.GetByNormalizedAsync(enNorm, ct);
            var isNewEn = false;
            if (en == null)
            {
                en = new EnglishWord { Text = dto.EnglishText };
                await _uow.EnglishWords.AddAsync(en, ct);
                isNewEn = true;
            }

            // Önce kaydedip Id almak isteyebiliriz (özellikle Relation eklemeden önce)
            await _uow.SaveChangesAsync(ct);
            result.EnglishWordId = en.Id;
            result.IsNewEnglishWord = isNewEn;

            // 2) TR anlamlar → TurkishWord + WordTranslation
            var addedTurkish = 0;
            var addedTranslations = 0;

            // Birincil anlam kontrolü
            var hasPrimary = await _uow.WordTranslations.GetPrimaryAsync(en.Id, ct) != null;
            var isFirstMeaning = true;

            foreach (var rawTr in dto.TurkishMeanings.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var trText = rawTr?.Trim();
                if (string.IsNullOrWhiteSpace(trText)) continue;

                var trNorm = trText.NormalizeForLookup();
                var tr = await _uow.TurkishWords.GetByNormalizedAsync(trNorm, ct);
                if (tr == null)
                {
                    tr = new TurkishWord { Text = trText! };
                    await _uow.TurkishWords.AddAsync(tr, ct);
                    addedTurkish++;
                }
                await _uow.SaveChangesAsync(ct);

                var exists = await _uow.WordTranslations.ExistsAsync(en.Id, tr.Id, ct);
                if (!exists)
                {
                    var trans = new WordTranslation
                    {
                        EnglishWordId = en.Id,
                        TurkishWordId = tr.Id,
                        IsPrimary = !hasPrimary && isFirstMeaning // en baştaki anlamı primary yap
                    };
                    await _uow.WordTranslations.AddAsync(trans, ct);
                    addedTranslations++;

                    if (trans.IsPrimary)
                        hasPrimary = true;
                }

                isFirstMeaning = false;
            }

            // 3) Cümleler (maksimum 2 tanesini sıralı ekleyelim)
            var addedSentences = 0;
            var sentences = dto.ExampleSentences
                               .Where(s => !string.IsNullOrWhiteSpace(s))
                               .Select(s => s.Trim())
                               .Distinct(StringComparer.OrdinalIgnoreCase)
                               .Take(2)
                               .ToList();

            if (sentences.Count > 0)
            {
                var existing = await _uow.ExampleSentences.GetByEnglishWordAsync(en.Id, ct);
                var existingSet = existing.Select(x => x.EnglishText.NormalizeForLookup()).ToHashSet();

                for (int i = 0; i < sentences.Count; i++)
                {
                    var st = sentences[i];
                    if (!existingSet.Contains(st.NormalizeForLookup()))
                    {
                        await _uow.ExampleSentences.AddAsync(new ExampleSentence
                        {
                            EnglishWordId = en.Id,
                            EnglishText = st,
                            OrderIndex = i
                        }, ct);
                        addedSentences++;
                    }
                }
            }

            // 4) Eş anlamlı EN kelimeler (EnglishWordRelation)
            var addedRelations = 0;
            foreach (var rawSyn in dto.EnglishSynonyms.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var synText = rawSyn?.Trim();
                if (string.IsNullOrWhiteSpace(synText)) continue;

                var synNorm = synText.NormalizeForLookup();
                var syn = await _uow.EnglishWords.GetByNormalizedAsync(synNorm, ct);
                if (syn == null)
                {
                    syn = new EnglishWord { Text = synText! };
                    await _uow.EnglishWords.AddAsync(syn, ct);
                    await _uow.SaveChangesAsync(ct);
                }

                // A–B / B–A simetrisi uygulama seviyesinde çözüyoruz
                if (!await _uow.EnglishWordRelations.RelationExistsAsync(en.Id, syn.Id, ct))
                {
                    int a = Math.Min(en.Id, syn.Id);
                    int b = Math.Max(en.Id, syn.Id);

                    await _uow.EnglishWordRelations.AddAsync(new EnglishWordRelation
                    {
                        EnglishWordId = a,
                        RelatedEnglishWordId = b
                    }, ct);
                    addedRelations++;
                }
            }

            // 5) Listeye ekleme (varsa)
            var addedToList = false;
            if (!string.IsNullOrWhiteSpace(dto.TargetListName))
            {
                var list = await _uow.WordLists.FirstOrDefaultAsync(x => x.Name == dto.TargetListName, ct);
                if (list == null)
                {
                    list = new WordList { Name = dto.TargetListName!.Trim() };
                    await _uow.WordLists.AddAsync(list, ct);
                    await _uow.SaveChangesAsync(ct);
                }

                var exists = await _uow.WordListItems.ExistsAsync(list.Id, en.Id, ct);
                if (!exists)
                {
                    await _uow.WordListItems.AddAsync(new WordListItem
                    {
                        WordListId = list.Id,
                        EnglishWordId = en.Id
                    }, ct);
                    addedToList = true;
                }
            }

            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            result.AddedTurkishCount = addedTurkish;
            result.AddedTranslationsCount = addedTranslations;
            result.AddedSentencesCount = addedSentences;
            result.AddedSynonymRelationsCount = addedRelations;
            result.AddedToList = addedToList;
            return result;
        }

        public async Task<List<UpsertResultDto>> UpsertBulkAsync(IEnumerable<WordUpsertDto> dtos, CancellationToken ct = default)
        {
            var results = new List<UpsertResultDto>();
            foreach (var dto in dtos)
            {
                var r = await UpsertSingleAsync(dto, ct);
                results.Add(r);
            }
            return results;
        }
    }
}
