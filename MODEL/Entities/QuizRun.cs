
using MODEL.Concretes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.Entities
{
    public class QuizRun : BaseEntity
    {
        public int WordListId { get; set; }
        public QuizMode Mode { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }

        public WordList WordList { get; set; } = null!;
        public string CycleOrderCsv { get; set; } = ""; 
        public int CycleIndex { get; set; } = 0;

        public bool IsPractice { get; set; } = false;   
        public int? SourceRunId { get; set; }

        public int SeedCount { get; set; }
        public string SeedWordIdsCsv { get; set; } = "";
    }

    public enum QuizMode
    {
        EnglishToTurkish = 0,
        TurkishToEnglish = 1
    }
}
