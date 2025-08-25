using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class QuizRunDetailDto
    {
        public int QuizRunId { get; set; }
        public int WordListId { get; set; }      // Replay'de lazım
        public string ListName { get; set; } = "";
        public bool IsPractice { get; set; }
        public QuizMode Mode { get; set; }
        public List<int> SeedIds { get; set; } = new();
        public List<WordAttemptRow> Words { get; set; } = new();

        public class WordAttemptRow
        {
            public int EnglishWordId { get; set; }
            public string English { get; set; } = "";
            public int Shown { get; set; }
            public int Correct { get; set; }
            public int Wrong { get; set; }
        }
    }
}
