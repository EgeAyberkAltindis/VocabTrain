using BLL.DTO;
using MODEL.Entities;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models.ViewModels
{
    public class StartQuizViewModel
    {
        [Display(Name = "Liste")]
        [Required]
        public int? SelectedListId { get; set; }  // artık ad değil Id

        public List<ListWithCountDto> Lists { get; set; } = new();

        [Display(Name = "Mod")]
        [Required]
        public QuizMode Mode { get; set; } = QuizMode.EnglishToTurkish;

       

        [Display(Name = "Soru Çekirdeği")]
        [Range(4, 50)]
        public int SeedCount { get; set; } = 10;

        public string? Message { get; set; }
    }
}
