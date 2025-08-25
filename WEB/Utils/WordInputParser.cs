using BLL.DTO;

namespace WEB.Utils
{
    public class WordInputParser
    {
        public static WordUpsertDto ParseSingle(string raw, string? targetListName = null)
        {
            // Trim + satırlara böl
            var lines = (raw ?? string.Empty)
                        .Replace("\r\n", "\n")
                        .Replace("\r", "\n")
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

            if (lines.Count == 0)
                throw new ArgumentException("Boş girdi.");

            if (lines.Count < 2)
                throw new ArgumentException("En az 2 satır olmalı: 1) EN: TR anlamlar, 2) eş anlamlar (boş olabilir), 3) örnek cümle(ler).");

            // 1. satır: English : tr1, tr2, ...
            var enAndTr = lines[0];
            var colonIdx = enAndTr.IndexOf(':');
            if (colonIdx < 0)
                throw new ArgumentException("1. satır 'Kelime : anlam1, anlam2' formatında olmalı.");

            var english = enAndTr.Substring(0, colonIdx).Trim().Trim('"');
            var trPart = enAndTr.Substring(colonIdx + 1).Trim();

            var trMeanings = trPart.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => x.Trim().Trim('"'))
                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();

            // 2. satır (opsiyonel): synonyms
            var synonyms = new List<string>();
            if (lines.Count >= 2)
            {
                // 2. satır eş anlamlar varsayılanı; boş bırakılabilir
                synonyms = lines[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(x => x.Trim().Trim('"'))
                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();
            }

            // 3. satır (opsiyonel): sentences "a / b"
            var sentences = new List<string>();
            if (lines.Count >= 3)
            {
                sentences = lines[2].Split('/', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim().Trim('"'))
                                    .Where(x => !string.IsNullOrWhiteSpace(x))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .Take(2)
                                    .ToList();
            }

            if (string.IsNullOrWhiteSpace(english))
                throw new ArgumentException("İngilizce kelime boş olamaz.");

            if (trMeanings.Count == 0)
                throw new ArgumentException("En az bir Türkçe anlam gerekli.");

            return new WordUpsertDto
            {
                EnglishText = english,
                TurkishMeanings = trMeanings,
                EnglishSynonyms = synonyms,
                ExampleSentences = sentences,
                TargetListName = targetListName
            };
        }

        // Çoklu kelime: boş satırlarla ayrılmış bloklar
        public static List<WordUpsertDto> ParseBulk(string raw, string? targetListName = null)
        {
            var blocks = (raw ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .ToList();

            var list = new List<WordUpsertDto>();
            foreach (var block in blocks)
            {
                list.Add(ParseSingle(block, targetListName));
            }
            return list;
        }
    }
}

