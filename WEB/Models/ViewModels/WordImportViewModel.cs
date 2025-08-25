using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels
{
    public class WordImportViewModel
    {
        [Display(Name = "Girdi (3 satır)")]
        [Required(ErrorMessage = "Girdi zorunludur.")]
        [MinLength(5, ErrorMessage = "Girdi çok kısa görünüyor.")]
        public string? Raw { get; set; }

        [Display(Name = "Hedef Liste Adı (opsiyonel)")]
        [StringLength(150, ErrorMessage = "Liste adı 150 karakteri geçemez.")]
        public string? TargetListName { get; set; }

        // Sonuç/geri bildirim
        public string? Message { get; set; }
    }
}
