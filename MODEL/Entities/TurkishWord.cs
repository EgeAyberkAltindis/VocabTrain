using MODEL.Concretes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class TurkishWord:BaseEntity
    {
        private string _text = null!;
        public string Text
        {
            get => _text;
            set
            {
                _text = (value ?? string.Empty).Trim();
                TextNormalized = _text.ToLowerInvariant();
            }
        }

        public string TextNormalized { get; private set; } = null!;
        public string? PartOfSpeech { get; set; }  // opsiyonel

        // Navigations
        public ICollection<WordTranslation> Translations { get; set; } = new List<WordTranslation>();
        // İleride Türkçe-Türkçe eş anlam istersen: TurkishWordRelation eklenebilir.
    }
}
