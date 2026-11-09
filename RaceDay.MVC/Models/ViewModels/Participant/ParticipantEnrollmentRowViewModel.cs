namespace RaceDay.MVC.Models.ViewModels.Participant;

public class ParticipantEnrollmentRowViewModel
{
    public int EnrollmentID { get; set; }
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public bool CanEdit { get; set; }
    public bool IsLocked => !CanEdit;
}
