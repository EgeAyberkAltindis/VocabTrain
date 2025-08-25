using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class QuizRunSummaryDto
    {
        public int QuizRunId { get; set; }
        public string ListName { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public bool IsPractice { get; set; }
        public QuizMode Mode { get; set; }
        public int SeedCount { get; set; }
        public int TotalShown { get; set; }
        public int Correct { get; set; }
        public int Wrong { get; set; }
    }
}
