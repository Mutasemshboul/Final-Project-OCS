using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_OCS.Models
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;


        // Navigation property to represent the one-to-many relationship
        public ICollection<StoreCategory> StoreCategories { get; set; } = new List<StoreCategory>();

    }
}
