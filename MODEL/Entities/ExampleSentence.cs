using MODEL.Concretes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class ExampleSentence:BaseEntity
    {
        public int EnglishWordId { get; set; }
        public string EnglishText { get; set; } = null!;
        public int OrderIndex { get; set; } = 0; // 0/1

        public EnglishWord EnglishWord { get; set; } = null!;
    }
}
