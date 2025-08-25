using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    namespace BLL.DTOs
    {
        public class WordExportDto
        {
            public string English { get; set; } = "";
            public List<string> TurkishMeanings { get; set; } = new();
            public List<string> Synonyms { get; set; } = new();
            public List<string> ExampleSentences { get; set; } = new();
        }
    }

}
