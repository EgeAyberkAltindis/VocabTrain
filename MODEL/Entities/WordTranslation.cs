using MODEL.Concretes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class WordTranslation:BaseEntity
    {
        public int EnglishWordId { get; set; }
        public int TurkishWordId { get; set; }

        // Birincil anlamı işaretlemek için (UI'da varsayılan doğru şık seçimi v.b.)
        public bool IsPrimary { get; set; }

        public EnglishWord EnglishWord { get; set; } = null!;
        public TurkishWord TurkishWord { get; set; } = null!;
    }
}
