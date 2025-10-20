using System;
using System.ComponentModel.DataAnnotations;

namespace CMCSApplication.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? LecturerName { get; set; }

        [Required]
        public string? Department { get; set; }

        [Required]
        public string? Month { get; set; }

        [Required]
        public int HoursWorked { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        // Add this property for your view
        public decimal TotalAmount
        {
            get
            {
                return HoursWorked * HourlyRate;
            }
        }

        // Add this to store the uploaded file path or name
        public string? SupportingDocument { get; set; }

        public string? Notes { get; set; }

        public string? Status { get; set; }

        public DateTime DateSubmitted { get; set; }
    }
}
