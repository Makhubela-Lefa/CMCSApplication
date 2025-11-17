using System.ComponentModel.DataAnnotations;

namespace CMCSApplication.Models
{
    public class Lecturer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        [Range(0, 2000)]
        public decimal HourlyRate { get; set; } = 0m;
        public string? Username { get; set; }
    }
}
