using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CMCSApplication.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Module name is required")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public ICollection<ModuleAssignment>? Assignments { get; set; }
    }
}
