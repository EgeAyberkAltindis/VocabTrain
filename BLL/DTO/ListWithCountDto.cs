using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class ListWithCountDto
    {
        public int WordListId { get; set; }
        public string Name { get; set; } = null!;
        public int WordCount { get; set; }
    }
}
