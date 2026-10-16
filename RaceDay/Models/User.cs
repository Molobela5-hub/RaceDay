
namespace RaceDay.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}
