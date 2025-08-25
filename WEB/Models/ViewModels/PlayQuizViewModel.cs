using MODEL.Entities;

namespace WEB.Models.ViewModels
{
    public class PlayQuizViewModel
    {
        public int QuizRunId { get; set; }
        public int WordListId { get; set; }
        public QuizMode Mode { get; set; }
    }
}
