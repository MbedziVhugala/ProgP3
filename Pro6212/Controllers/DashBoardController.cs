using Microsoft.AspNetCore.Mvc;
using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;
using ClaimModel = Prog6212.Models.Claim;
using System.Security.Claims;


namespace Prog6212.Controllers
{
        public class DashboardController : Controller
        {
            private readonly IDataService _dataService;

            public DashboardController(IDataService dataService)
            {
                _dataService = dataService;
            }

            public async Task<IActionResult> Index()
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Login", "Account");

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                var claims = await _dataService.GetClaimsAsync();
                var users = await _dataService.GetUsersAsync();

                var viewModel = new DashboardViewModel
                {
                    UserName = userName,
                    UserRole = userRole
                };

                if (userRole == "Lecturer")
                {
                    var lecturerClaims = claims.Where(c => c.UserId == userId).ToList();

                    viewModel.RecentClaims = lecturerClaims
                        .OrderByDescending(c => c.SubmissionDate)
                        .Take(10)
                        .Select(c => new ClaimSummaryViewModel
                        {
                            Id = c.Id,
                            LecturerName = c.User?.FullName,
                            TotalAmount = c.TotalAmount,
                            Status = c.Status,
                            SubmissionDate = c.SubmissionDate,
                            HasDocument = !string.IsNullOrEmpty(c.SupportingDocument)
                        })
                        .ToList();

                    viewModel.Stats = new DashboardStatsViewModel
                    {
                        TotalClaims = lecturerClaims.Count,
                        PendingClaims = lecturerClaims.Count(c => c.Status == "Pending"),
                        ApprovedClaims = lecturerClaims.Count(c => c.Status == "Approved"),
                        RejectedClaims = lecturerClaims.Count(c => c.Status == "Rejected"),
                        TotalAmountApproved = lecturerClaims
                            .Where(c => c.Status == "Approved")
                            .Sum(c => c.TotalAmount)
                    };
                }
                else if (userRole == "Coordinator" || userRole == "Manager")
                {
                    var pendingClaims = claims.Where(c => c.Status == "Pending").ToList();

                    viewModel.PendingApprovals = pendingClaims
                        .OrderBy(c => c.SubmissionDate)
                        .Take(10)
                        .Select(c => new ClaimSummaryViewModel
                        {
                            Id = c.Id,
                            LecturerName = c.User?.FullName,
                            TotalAmount = c.TotalAmount,
                            Status = c.Status,
                            SubmissionDate = c.SubmissionDate,
                            HasDocument = !string.IsNullOrEmpty(c.SupportingDocument)
                        })
                        .ToList();

                    viewModel.Stats = new DashboardStatsViewModel
                    {
                        TotalClaims = claims.Count,
                        PendingClaims = pendingClaims.Count,
                        ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                        RejectedClaims = claims.Count(c => c.Status == "Rejected")
                    };
                }

                return View(viewModel);
            }
        }
    }