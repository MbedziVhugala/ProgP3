using System.Text.Json;
using Prog6212.Models;
using Microsoft.AspNetCore.Hosting; // Required for IWebHostEnvironment
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Alias to disambiguate between your model and System.Security.Claims.Claim
using ClaimModel = Prog6212.Models.Claim;

namespace Prog6212.Services
{
    public interface IDataService
    {
        Task<List<User>> GetUsersAsync();
        Task<List<ClaimModel>> GetClaimsAsync();
        Task<ClaimModel> AddClaimAsync(ClaimModel claim);
        Task<ClaimModel> UpdateClaimAsync(ClaimModel claim);
        Task<User> GetUserAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task SaveClaimsAsync(List<ClaimModel> claims);
    }

    public class DataService : IDataService
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _options;

        private List<User> _users = new();
        private List<ClaimModel> _claims = new();

        public DataService(IWebHostEnvironment environment)
        {
            _dataPath = Path.Combine(environment.ContentRootPath, "App_Data");
            _options = new JsonSerializerOptions { WriteIndented = true };

            Directory.CreateDirectory(_dataPath);
            InitializeDataAsync().Wait();
        }

        private async Task InitializeDataAsync()
        {
            _users = await LoadUsersAsync();
            _claims = await LoadClaimsAsync();

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
        private async Task<List<User>> LoadUsersAsync()
        {
            var filePath = Path.Combine(_dataPath, "users.json");
            if (!File.Exists(filePath))
            {
                var defaultUsers = new List<User>
                {
                    new User { Id = 1, Email = "lecturer@iie.ac.za", Password = "lecturer123",
                              FullName = "Dr. John Smith", Role = "Lecturer" },
                    new User { Id = 2, Email = "coordinator@iie.ac.za", Password = "coordinator123",
                              FullName = "Ms. Sarah Wilson", Role = "Coordinator" },
                    new User { Id = 3, Email = "manager@iie.ac.za", Password = "manager123",
                              FullName = "Mr. David Brown", Role = "Manager" },
                    new User { Id = 4, Email = "lecturer2@iie.ac.za", Password = "lecturer123",
                              FullName = "Prof. Emily Johnson", Role = "Lecturer" }
                };
                await SaveUsersAsync(defaultUsers);
                return defaultUsers;
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private async Task SaveUsersAsync(List<User> users)
        {
            var filePath = Path.Combine(_dataPath, "users.json");
            var json = JsonSerializer.Serialize(users, _options);
            await File.WriteAllTextAsync(filePath, json);
        }

        // ================================
        // CLAIM METHODS
        // ================================
        private async Task<List<ClaimModel>> LoadClaimsAsync()
        {
            var filePath = Path.Combine(_dataPath, "claims.json");
            if (!File.Exists(filePath))
            {
                var defaultClaims = new List<ClaimModel>
                {
                    new ClaimModel
                    {
                        Id = 1, UserId = 1, HoursWorked = 40, HourlyRate = 250,
                        AdditionalNotes = "Monthly teaching hours", Status = "Approved",
                        SubmissionDate = DateTime.Now.AddDays(-10), ApprovalDate = DateTime.Now.AddDays(-5),
                        ApprovedBy = 2
                    },
                    new ClaimModel
                    {
                        Id = 2, UserId = 1, HoursWorked = 35, HourlyRate = 250,
                        AdditionalNotes = "Student consultations", Status = "Pending",
                        SubmissionDate = DateTime.Now.AddDays(-3)
                    },
                    new ClaimModel
                    {
                        Id = 3, UserId = 4, HoursWorked = 45, HourlyRate = 280,
                        AdditionalNotes = "Research supervision", Status = "Pending",
                        SubmissionDate = DateTime.Now.AddDays(-1)
                    }
                };
                await SaveClaimsAsync(defaultClaims);
                return defaultClaims;
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<ClaimModel>>(json) ?? new List<ClaimModel>();
        }

        public async Task SaveClaimsAsync(List<ClaimModel> claims)
        {
            var filePath = Path.Combine(_dataPath, "claims.json");
            var json = JsonSerializer.Serialize(claims, _options);
            await File.WriteAllTextAsync(filePath, json);
        }

        // ================================
        // PUBLIC INTERFACE IMPLEMENTATION
        // ================================
        public async Task<List<User>> GetUsersAsync() => _users;

        public async Task<List<ClaimModel>> GetClaimsAsync() => _claims;

        public async Task<ClaimModel> AddClaimAsync(ClaimModel claim)
        {
            claim.Id = _claims.Any() ? _claims.Max(c => c.Id) + 1 : 1;
            claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
            _claims.Add(claim);
            await SaveClaimsAsync(_claims);
            return claim;
        }

        public async Task<ClaimModel> UpdateClaimAsync(ClaimModel updatedClaim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
            if (existingClaim != null)
            {
                _claims.Remove(existingClaim);
                updatedClaim.User = _users.FirstOrDefault(u => u.Id == updatedClaim.UserId);
                if (updatedClaim.ApprovedBy.HasValue)
                {
                    updatedClaim.Approver = _users.FirstOrDefault(u => u.Id == updatedClaim.ApprovedBy.Value);
                }
                _claims.Add(updatedClaim);
                await SaveClaimsAsync(_claims);
            }
            return updatedClaim;
        }

        public async Task<User> GetUserAsync(int id) =>
            _users.FirstOrDefault(u => u.Id == id);

        public async Task<User> GetUserByEmailAsync(string email) =>
            _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
