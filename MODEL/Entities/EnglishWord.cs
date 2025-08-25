using MODEL.Concretes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class EnglishWord:BaseEntity
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
        public string? PartOfSpeech { get; set; }  // "verb", "adj" vb.
        public string? Level { get; set; }         // "A1", "B2" vb.

        // Navigations
        public ICollection<WordTranslation> Translations { get; set; } = new List<WordTranslation>();
        public ICollection<EnglishWordRelation> RelationsFrom { get; set; } = new List<EnglishWordRelation>();
        public ICollection<EnglishWordRelation> RelationsTo { get; set; } = new List<EnglishWordRelation>();
        public ICollection<ExampleSentence> Sentences { get; set; } = new List<ExampleSentence>();
        public ICollection<WordListItem> WordListItems { get; set; } = new List<WordListItem>();
    }
}
