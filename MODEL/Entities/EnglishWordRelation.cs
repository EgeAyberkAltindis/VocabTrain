using MODEL.Concretes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class EnglishWordRelation:BaseEntity
    {
        public int EnglishWordId { get; set; }
        public int RelatedEnglishWordId { get; set; }
       

        public EnglishWord EnglishWord { get; set; } = null!;
        public EnglishWord RelatedEnglishWord { get; set; } = null!;
    }
}
