using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels
{
    public class BulkWordImportViewModel
    {
        [Display(Name = "Girdi (bloklar arası boş satır)")]
        [Required(ErrorMessage = "Girdi zorunludur.")]
        [MinLength(5)]
        public string? Raw { get; set; }

        [Display(Name = "Hedef Liste Adı (opsiyonel)")]
        [StringLength(150)]
        public string? TargetListName { get; set; }

        public string? Message { get; set; }
    }
}