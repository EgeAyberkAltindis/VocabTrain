using BLL.DTO;

namespace WEB.Models.ViewModels
{
    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; } // 1..12
        public DateOnly SelectedDay { get; set; }

        public List<DayCell> Days { get; set; } = new();
        public List<QuizRunSummaryDto> Runs { get; set; } = new();

        public class DayCell
        {
            public DateOnly Date { get; set; }
            public int DayNumber => Date.Day;
            public bool InMonth { get; set; }
            public bool IsToday { get; set; }
            public bool IsSelected { get; set; }
            public int Count { get; set; }
            public int? SpecialAgo { get; set; } // 1,3,7,15 ise kırmızı yuvarlak
        }

        public List<DateOnly> StripDays { get; set; } = new(); // seçili gün ve komşuları
    }
}
