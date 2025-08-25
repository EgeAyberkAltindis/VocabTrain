using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class WordHintDto
    {
        public List<string> Meanings { get; set; } = new(); // mode'a göre TR ya da EN
        public List<string> Synonyms { get; set; } = new();
    }
}
