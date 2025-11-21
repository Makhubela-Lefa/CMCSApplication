using System.ComponentModel.DataAnnotations;

namespace CMCSApplication.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 2000)]
        public decimal HourlyRate { get; set; }
    }
}
