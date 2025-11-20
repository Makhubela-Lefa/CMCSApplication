using System.ComponentModel.DataAnnotations;

namespace CMCSApplication.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; // For POE: plain text is fine

        [Required]
        public string Role { get; set; } = string.Empty; // HR, Manager, Coordinator, Lecturer

        // Basic profile fields
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Lecturer-specific
        public decimal? HourlyRate { get; set; } // Only for Lecturers

        // Link to Lecturer table 
        public int? LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }
    }
}
