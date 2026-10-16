
namespace RaceDay.Models
{
    public class Result
    {
        public int ResultID { get; set; }

        public int EnrollmentID { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int FinishingPosition { get; set; }
    }
}
