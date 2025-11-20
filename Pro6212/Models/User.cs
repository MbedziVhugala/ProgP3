using System.ComponentModel.DataAnnotations;

namespace Prog6212.Models
{

        public class User
        {
            public int Id { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }

            [Required]
            public string FullName { get; set; }

            [Required]
            public string Role { get; set; } // Lecturer, Coordinator, Manager

            public bool IsActive { get; set; } = true;
            public DateTime CreatedDate { get; set; } = DateTime.Now;
        }
    }