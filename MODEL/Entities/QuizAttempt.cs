using MODEL.Concretes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class QuizAttempt : BaseEntity
    {
        public int QuizRunId { get; set; }
        public int EnglishWordId { get; set; }
        public bool IsCorrect { get; set; }

        public QuizRun QuizRun { get; set; } = null!;
        public EnglishWord EnglishWord { get; set; } = null!;
    }
}
