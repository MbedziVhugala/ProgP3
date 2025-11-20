using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClaimModel = Prog6212.Models.Claim;
using System.Security.Claims;

namespace Prog6212.Controllers
{
    public class ApprovalController : Controller
    {
        private readonly IDataService _dataService;

        public ApprovalController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var claims = await _dataService.GetClaimsAsync();
            var claim = claims.FirstOrDefault(c => c.Id == id);

            if (claim == null)
                return NotFound();

            var viewModel = new ClaimViewModel
            {
                Id = claim.Id,
                LecturerName = claim.User?.FullName,
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                AdditionalNotes = claim.AdditionalNotes,
                SupportingDocumentPath = claim.SupportingDocument,
                SubmissionDate = claim.SubmissionDate
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var claims = await _dataService.GetClaimsAsync();
            var claim = claims.FirstOrDefault(c => c.Id == id);

            if (claim == null)
                return NotFound();

            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            claim.Status = "Approved";
            claim.ApprovalDate = DateTime.Now;
            claim.ApprovedBy = approverId;

            await _dataService.UpdateClaimAsync(claim);

            TempData["SuccessMessage"] = $"Claim #{claim.Id} approved successfully!";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var claims = await _dataService.GetClaimsAsync();
            var claim = claims.FirstOrDefault(c => c.Id == id);

            if (claim == null)
                return NotFound();

            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            claim.Status = "Rejected";
            claim.ApprovalDate = DateTime.Now;
            claim.ApprovedBy = approverId;
            claim.AdditionalNotes += $"\n\nRejection Reason: {rejectionReason}";

            await _dataService.UpdateClaimAsync(claim);

            TempData["SuccessMessage"] = $"Claim #{claim.Id} has been rejected.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
