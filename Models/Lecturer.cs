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
    }
}
