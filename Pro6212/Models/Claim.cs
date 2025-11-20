using System;
using System.ComponentModel.DataAnnotations;

namespace Prog6212.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 200)]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal HourlyRate { get; set; }

        public decimal TotalAmount => HoursWorked * HourlyRate;

        [StringLength(1000)]
        public string AdditionalNotes { get; set; }

        public string SupportingDocument { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
        public int? ApprovedBy { get; set; }

        // Navigation properties
        public User User { get; set; }
        public User Approver { get; set; }
    }
}
