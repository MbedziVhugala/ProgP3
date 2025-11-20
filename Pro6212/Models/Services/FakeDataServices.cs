using Prog6212.Models;
using Prog6212.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Alias to avoid conflict
using ClaimModel = Prog6212.Models.Claim;

namespace Prog6212.Services
{
    public class FakeDataService : IDataService
    {
        private readonly List<User> _users;
        private readonly List<ClaimModel> _claims;

        public FakeDataService()
        {
            // Initialize with some test users
            _users = new List<User>
            {
                new User { Id = 1, Email = "lecturer@example.com", Password = "Test@123", FullName = "Dr. John Smith", Role = "Lecturer" },
                new User { Id = 2, Email = "coordinator@example.com", Password = "Test@123", FullName = "Ms. Sarah Wilson", Role = "Coordinator" },
                new User { Id = 3, Email = "manager@example.com", Password = "Test@123", FullName = "Mr. David Brown", Role = "Manager" }
            };

            // Initialize some claims
            _claims = new List<ClaimModel>
            {
                new ClaimModel { Id = 1, UserId = 1, HoursWorked = 40, HourlyRate = 250, AdditionalNotes = "Monthly teaching", Status = "Pending", SubmissionDate = DateTime.Now.AddDays(-3) },
                new ClaimModel { Id = 2, UserId = 1, HoursWorked = 35, HourlyRate = 250, AdditionalNotes = "Student consultations", Status = "Approved", SubmissionDate = DateTime.Now.AddDays(-10), ApprovalDate = DateTime.Now.AddDays(-5), ApprovedBy = 2 }
            };

            // Populate navigation properties
            foreach (var claim in _claims)
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
                if (claim.ApprovedBy.HasValue)
                {
                    claim.Approver = _users.FirstOrDefault(u => u.Id == claim.ApprovedBy.Value);
                }
            }
        }

        // ================================
        // USER METHODS
        // ================================
        public Task<List<User>> GetUsersAsync() => Task.FromResult(_users);

        public Task<User> GetUserAsync(int id) => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task<User> GetUserByEmailAsync(string email) =>
            Task.FromResult(_users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task<User> AddUserAsync(User user)
        {
            user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
            return Task.FromResult(user);
        }

        // ================================
        // CLAIM METHODS
        // ================================
        public Task<List<ClaimModel>> GetClaimsAsync() => Task.FromResult(_claims);

        public Task<ClaimModel> AddClaimAsync(ClaimModel claim)
        {
            claim.Id = _claims.Any() ? _claims.Max(c => c.Id) + 1 : 1;
            claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
            _claims.Add(claim);
            return Task.FromResult(claim);
        }

        public Task<ClaimModel> UpdateClaimAsync(ClaimModel updatedClaim)
        {
            var existing = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
            if (existing != null)
            {
                _claims.Remove(existing);
                updatedClaim.User = _users.FirstOrDefault(u => u.Id == updatedClaim.UserId);
                if (updatedClaim.ApprovedBy.HasValue)
                {
                    updatedClaim.Approver = _users.FirstOrDefault(u => u.Id == updatedClaim.ApprovedBy.Value);
                }
                _claims.Add(updatedClaim);
            }
            return Task.FromResult(updatedClaim);
        }

        public Task SaveClaimsAsync(List<ClaimModel> claims)
        {
            _claims.Clear();
            _claims.AddRange(claims);
            return Task.CompletedTask;
        }
    }
}
