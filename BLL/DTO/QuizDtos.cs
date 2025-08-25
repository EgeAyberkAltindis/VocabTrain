
using MODEL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class QuizDtos
    {
        public class StartQuizRequest
        {
            public int WordListId { get; set; }
            public QuizMode Mode { get; set; }     // EN->TR veya TR->EN

            
            
            public int SeedCount { get; set; } = 10;           // 10 kelimelik çekirdek set
        }

        /// <summary>
        /// Quiz başlatıldığında dönen yanıt: oluşturulan QuizRunId ve seed EnglishWord id’leri.
        /// UI bu id’lerle sıradaki soruları üretir/ister.
        /// </summary>
        public class StartQuizResult
        {
            public int QuizRunId { get; set; }
            public int WordListId { get; set; }
            public QuizMode Mode { get; set; }

            /// <summary>
            /// Başlangıç çekirdeğini oluşturan EnglishWord Id listesi (10 önerilir).
            /// </summary>
            public List<int> SeedEnglishWordIds { get; set; } = new();
        }

        /// <summary>
        /// UI’ye gösterilecek tek bir soru paketi.
        /// Prompt: gösterilecek metin (EN veya TR),
        /// Options: 4 şık,
        /// CorrectIndex: doğru şıkkın 0..3 arasındaki indeksi.
        /// </summary>
        public class QuizQuestionDto
        {
            public int QuizRunId { get; set; }
            public int EnglishWordId { get; set; }
            public QuizMode Mode { get; set; }

            public string Prompt { get; set; } = null!;
            public List<string> Options { get; set; } = new();
            public int CorrectIndex { get; set; }
        }

        /// <summary>
        /// UI’nin "şu soruda şu şıkkı seçtim" demesi için gönderdiği paket.
        /// </summary>
        public class SubmitAnswerRequest
        {

            public int QuizRunId { get; set; }
            public int EnglishWordId { get; set; }

            // UI'da tıklanan şık metni (EN->TR'de Türkçe metin, TR->EN'de İngilizce metin)
            public string SelectedText { get; set; } = null!;
           
        }

        /// <summary>
        /// Cevap değerlendirme sonucu (doğru/yanlış + doğru cevap metni).
        /// UI anlık geri bildirim gösterebilir.
        /// </summary>
        public class SubmitAnswerResult
        {
            public bool IsCorrect { get; set; }
            public string? CorrectAnswer { get; set; }
        }

        /// <summary>
        /// Quiz bitiş özeti: toplam gösterim, toplam doğru/yanlış ve kelime bazında döküm.
        /// </summary>
        public class QuizSummaryDto
        {
            public int QuizRunId { get; set; }
            public int TotalShown { get; set; }
            public int CorrectCount { get; set; }
            public int WrongCount { get; set; }

            public List<QuizWordSummaryItem> Items { get; set; } = new();
        }

        /// <summary>
        /// Her bir kelime için kaç kez gösterildi, kaç doğru/yanlış yapıldı.
        /// </summary>
        public class QuizWordSummaryItem
        {
            public int EnglishWordId { get; set; }
            public string EnglishText { get; set; } = null!;
            public int TimesShown { get; set; }
            public int Correct { get; set; }
            public int Wrong { get; set; }
        }

        
    }
}
