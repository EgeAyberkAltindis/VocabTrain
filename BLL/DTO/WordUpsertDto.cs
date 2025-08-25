using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    /// <summary>
    /// Web katmanındaki parser’dan BLL’e gelen tek kelimelik paket.
    /// BLL bu veriyi kullanarak EnglishWord/TurkishWord/WordTranslation/ExampleSentence/Relation kayıtlarını upsert eder.
    /// </summary>
    public class WordUpsertDto
    {
        /// <summary>
        /// "Drastically" gibi İngilizce kök kelime.
        /// </summary>
        public string EnglishText { get; set; } = null!;

        /// <summary>
        /// Türkçe anlamlar listesi (örn. "büyük ölçüde", "ciddi şekilde"...).
        /// Web parse aşamasında virgüllerden parçalanır.
        /// </summary>
        public List<string> TurkishMeanings { get; set; } = new();

        /// <summary>
        /// İngilizce eş anlamlılar (ör. "significantly", "severely"...).
        /// Bunlar EnglishWordRelation ile EN↔EN bağ kurmak için kullanılır.
        /// </summary>
        public List<string> EnglishSynonyms { get; set; } = new();

        /// <summary>
        /// En fazla 2 adet örnek cümle. Slash'la ayrılmış gelen metinler parser’da bölünüp buraya konur.
        /// </summary>
        public List<string> ExampleSentences { get; set; } = new();

        /// <summary>
        /// Kelimenin ekleneceği liste adı (ör. "Sıfatlar", "Top 1000"...).
        /// BLL, liste yoksa oluşturur; varsa kelimeyi o listeye ekler.
        /// </summary>
        public string? TargetListName { get; set; }
    }
}
