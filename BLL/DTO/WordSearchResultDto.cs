using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class WordSearchResultDto
    {
        public int EnglishWordId { get; set; }
        public string EnglishText { get; set; } = null!;
        public List<string> TurkishMeanings { get; set; } = new(); // Primary önce
        public List<string> Synonyms { get; set; } = new();
        public List<string> ExampleSentences { get; set; } = new(); // en fazla 2
    }
}
