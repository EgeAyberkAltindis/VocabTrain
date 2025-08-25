using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class StartFromSeedRequest
    {
        public int WordListId { get; set; }
        public QuizMode Mode { get; set; }
        public List<int> SeedIds { get; set; } = new();
        public bool IsPractice { get; set; } = false;
        public int? SourceRunId { get; set; }
    }
}
