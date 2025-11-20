using Microsoft.AspNetCore.Mvc;
using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;
using System.Security.Claims;                // For logged-in user info
using ClaimModel = Prog6212.Models.Claim;   // Alias your model to avoid confusion


namespace Prog6212.Controllers



{

    public class ClaimController : Controller
    {
        public int Id { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public string AdditionalNotes { get; set; }
        public string LecturerName { get; set; }
        public string Status { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string SupportingDocumentPath { get; set; }
        public IFormFile SupportingDocument { get; set; }

        private readonly IDataService _dataService;
        private readonly IWebHostEnvironment _environment;

        public ClaimController(IDataService dataService, IWebHostEnvironment environment)
        {
            _dataService = dataService;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ClaimViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClaimViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var claim = new ClaimModel
                {
                    UserId = userId,
                    HoursWorked = viewModel.HoursWorked,
                    HourlyRate = viewModel.HourlyRate,
                    AdditionalNotes = viewModel.AdditionalNotes,
                    Status = "Pending",
                    SubmissionDate = DateTime.Now
                };

                // Handle file upload
                if (viewModel.SupportingDocument != null && viewModel.SupportingDocument.Length > 0)
                {
                    claim.SupportingDocument = await SaveDocumentAsync(viewModel.SupportingDocument);
                }

                await _dataService.AddClaimAsync(claim);

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while submitting the claim. Please try again.");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Lecturer"))
                return RedirectToAction("AccessDenied", "Account");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var claims = await _dataService.GetClaimsAsync();
            var userClaims = claims.Where(c => c.UserId == userId).ToList();

            var viewModels = userClaims.Select(c => new ClaimViewModel
            {
                Id = c.Id,
                HoursWorked = c.HoursWorked,
                HourlyRate = c.HourlyRate,
                AdditionalNotes = c.AdditionalNotes,
                LecturerName = c.User?.FullName,
                Status = c.Status,
                SubmissionDate = c.SubmissionDate,
                SupportingDocumentPath = c.SupportingDocument
            }).ToList();


            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var claims = await _dataService.GetClaimsAsync();
            var claim = claims.FirstOrDefault(c => c.Id == id);

            if (claim == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (claim.UserId != userId && !User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var viewModel = new ClaimViewModel
            {
                Id = claim.Id,
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                AdditionalNotes = claim.AdditionalNotes,
                LecturerName = claim.User?.FullName,
                Status = claim.Status,
                SubmissionDate = claim.SubmissionDate,
                SupportingDocumentPath = claim.SupportingDocument
            };


            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int id)
        {
            if (!User.IsInRole("Lecturer"))
                return RedirectToAction("AccessDenied", "Account");

            var claims = await _dataService.GetClaimsAsync();
            var claim = claims.FirstOrDefault(c => c.Id == id);

            if (claim == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (claim.UserId != userId)
                return RedirectToAction("AccessDenied", "Account");

            if (claim.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending claims can be withdrawn.";
                return RedirectToAction("Details", new { id });
            }

            // Remove the claim
            claims.Remove(claim);
            await _dataService.SaveClaimsAsync(claims);

            TempData["SuccessMessage"] = "Claim withdrawn successfully.";
            return RedirectToAction("Index");
        }

        private async Task<string> SaveDocumentAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException("Invalid file type. Allowed: PDF, DOCX, XLSX, JPG, PNG");

            if (file.Length > 10 * 1024 * 1024) // 10MB
                throw new InvalidOperationException("File size exceeds 10MB limit");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}