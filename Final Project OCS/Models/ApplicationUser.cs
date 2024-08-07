using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Final_Project_OCS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        public string Address { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public int Points { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
    }
}