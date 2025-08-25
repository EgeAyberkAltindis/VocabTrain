using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    /// <summary>
    /// Upsert işleminin özet sonucu: hangi kayıtlar yeni eklendi, hangileri zaten vardı gibi bilgiler.
    /// UI’de bilgilendirme/geri bildirim için kullanılır.
    /// </summary>
    public class UpsertResultDto
    {
        public int EnglishWordId { get; set; }
        public bool IsNewEnglishWord { get; set; }

        public string EnglishText { get; set; } = null!;
        public string? TargetListName { get; set; }

        public int AddedTurkishCount { get; set; }
        public int AddedTranslationsCount { get; set; }
        public int AddedSentencesCount { get; set; }
        public int AddedSynonymRelationsCount { get; set; }

        /// <summary>
        /// Kelime hedef listeye yeni eklendiyse true (zaten varsa false).
        /// </summary>
        public bool AddedToList { get; set; }
    }
    
}
