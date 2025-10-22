using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CMCSApplication.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Lecturer name is required")]
        public string LecturerName { get; set; } = string.Empty;

        [ForeignKey("Lecturer")]
        public int? LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }


        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Month is required")]
        public string Month { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours must be between 1 and 200")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(1, 2000, ErrorMessage = "Rate must be between 1 and 2000")]
        [DataType(DataType.Currency)]
        public decimal HourlyRate { get; set; }

        [NotMapped] // Not stored directly in DB
        public decimal TotalAmount => HoursWorked * HourlyRate;

        [Display(Name = "Supporting Document Path")]
        public string? SupportingDocument { get; set; }

        [Display(Name = "Original File Name")]
        public string? OriginalFileName { get; set; }
        public string FilePath { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Claim Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Coordinator Status")]
        public string CoordinatorStatus { get; set; } = "Pending Verification";

        [Display(Name = "Manager Status")]
        public string ManagerStatus { get; set; } = "Pending Approval";


        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        [Display(Name = "Date Verified")]
        public DateTime? DateVerified { get; set; }

        [Display(Name = "Date Approved")]
        public DateTime? DateApproved { get; set; }

        // This is for file upload (used in forms, not DB)
        [NotMapped]
        [Display(Name = "Upload Document")]
        public IFormFile? UploadFile { get; set; }
        public int? ModuleId { get; set; }   // foreign key
        public Module? Module { get; set; }  // navigation property

      
    }
}
