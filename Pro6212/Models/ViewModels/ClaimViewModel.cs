using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Prog6212.ViewModels
{
    public class ClaimViewModel
    {
        // Core properties
        public int Id { get; set; }  // Used by ClaimController and DocumentController
        public int ClaimId => Id;    // Used by ReviewClaim.cshtml (for backward compatibility)

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0, 1000, ErrorMessage = "Hourly rate must be between 0 and 1000")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Additional Notes")]
        public string? AdditionalNotes { get; set; }

        [Display(Name = "Supporting Document")]
        public IFormFile? SupportingDocument { get; set; }  // For uploading

        public string? SupportingDocumentPath { get; set; }  // For displaying/saving filename

        public string? LecturerName { get; set; }
        public string? Status { get; set; }
        public DateTime SubmissionDate { get; set; }

        // Computed property (auto-calculated)
        public decimal TotalAmount => HoursWorked * HourlyRate;

        // Optional (used in approval/rejection)
        public string? RejectionReason { get; set; }
    }
}
