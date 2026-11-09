using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDay.MVC.Models.ViewModels.Participant;

public class ParticipantEditEnrollmentViewModel
{
    [Required]
    public int EnrollmentID { get; set; }

    public int EventID { get; set; }

    public string EventName { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a category.")]
    [Display(Name = "Category")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryID { get; set; }

    public List<SelectListItem> CategoryOptions { get; set; } = [];
}
