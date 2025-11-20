using Xunit;
using Microsoft.AspNetCore.Mvc;
using Prog6212.Controllers;
using Prog6212.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace Prog6212.Tests
{
    public class FastTests
    {
        // =======================
        // 1. Login succeeds
        // =======================
        [Fact]
        public async Task AccountController_Login_ValidUser_RedirectsToDashboard()
        {
            var controller = new AccountController();

            var result = await controller.Login("lecturer@example.com", "Test@123") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Dashboard", result.ControllerName);
            Assert.Equal("Index", result.ActionName);
        }

        // =======================
        // 2. Login fails
        // =======================
        [Fact]
        public async Task AccountController_Login_InvalidUser_ReturnsViewWithError()
        {
            var controller = new AccountController();

            var result = await controller.Login("wrong@example.com", "abc") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("Invalid email or password.", controller.ViewBag.Error);
        }

        // =======================
        // 3. Claim creation redirects
        // =======================
        [Fact]
        public async Task ClaimController_CreateClaim_ValidModel_Redirects()
        {
            var controller = new ClaimController(null, null); // No IDataService needed for this minimal test
            var viewModel = new ClaimViewModel { HoursWorked = 10, HourlyRate = 100 };

            // Mock logged-in user
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1")
                    }))
                }
            };

            var result = await controller.Create(viewModel) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        // =======================
        // 4. Approval sets claim status
        // =======================
        /* [Fact]
         public async Task ApprovalController_Approve_PendingClaim_StatusChanged()
         {
             var controller = new ApprovalController(new FakeDataService());

             // Mock coordinator user
             controller.ControllerContext = new ControllerContext
             {
                 HttpContext = new DefaultHttpContext
                 {
                     User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                     {
                         new Claim(ClaimTypes.NameIdentifier, "2"),
                         new Claim(ClaimTypes.Role, "Coordinator")
                     }))
                 }
             };

             var result = await controller.Approve(1) as RedirectToActionResult;

             Assert.NotNull(result);
             Assert.Equal("Dashboard", result.ControllerName);

             var claim = (await controller._dataService.GetClaimsAsync()).First(c => c.Id == 1);
             Assert.Equal("Approved", claim.Status);
         }*/

        // =======================
        // 5. Reject sets claim status and reason
        // =======================
        /*  [Fact]
          public async Task ApprovalController_Reject_PendingClaim_StatusRejected()
          {
              var controller = new ApprovalController(new FakeDataService());

              controller.ControllerContext = new ControllerContext
              {
                  HttpContext = new DefaultHttpContext
                  {
                      User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                      {
                          new Claim(ClaimTypes.NameIdentifier, "2"),
                          new Claim(ClaimTypes.Role, "Coordinator")
                      }))
                  }
              };

              var result = await controller.Reject(2, "Invalid data") as RedirectToActionResult;

              Assert.NotNull(result);
              var claim = (await controller._dataService.GetClaimsAsync()).First(c => c.Id == 2);
              Assert.Equal("Rejected", claim.Status);
              Assert.Contains("Invalid data", claim.AdditionalNotes);
          }
      }*/

        // Minimal fake service for tests
        public class FakeDataService : Prog6212.Services.IDataService
        {
            private List<Prog6212.Models.Claim> _claims = new()
        {
            new Prog6212.Models.Claim { Id = 1, UserId = 1, Status = "Pending", AdditionalNotes = "" },
            new Prog6212.Models.Claim { Id = 2, UserId = 1, Status = "Pending", AdditionalNotes = "" }
        };

            public Task<List<Prog6212.Models.Claim>> GetClaimsAsync() => Task.FromResult(_claims);
            public Task<List<Prog6212.Models.User>> GetUsersAsync() => Task.FromResult(new List<Prog6212.Models.User>());
            public Task<Prog6212.Models.Claim> AddClaimAsync(Prog6212.Models.Claim claim)
            {
                _claims.Add(claim);
                return Task.FromResult(claim);
            }
            public Task<Prog6212.Models.Claim> UpdateClaimAsync(Prog6212.Models.Claim claim)
            {
                var existing = _claims.First(c => c.Id == claim.Id);
                existing.Status = claim.Status;
                existing.AdditionalNotes = claim.AdditionalNotes;
                return Task.FromResult(existing);
            }
            public Task<Prog6212.Models.User> GetUserAsync(int id) => Task.FromResult(new Prog6212.Models.User());
            public Task<Prog6212.Models.User> GetUserByEmailAsync(string email) => Task.FromResult(new Prog6212.Models.User());
            public Task SaveClaimsAsync(List<Prog6212.Models.Claim> claims) => Task.CompletedTask;
        }
    }
}
