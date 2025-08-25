using MODEL.Concretes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class WordStat : BaseEntity
    {
        public int WordListId { get; set; }
        public int EnglishWordId { get; set; }

        public int TimesShown { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public DateTime? LastShownAt { get; set; }

        public WordList WordList { get; set; } = null!;
        public EnglishWord EnglishWord { get; set; } = null!;
    }
}
