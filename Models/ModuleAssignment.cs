using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CMCSApplication.Models;


namespace CMCSApplication.Models
{
    public class ModuleAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }

        [Required]
        public int ModuleId { get; set; }
        public Module? Module { get; set; }
    }
}
