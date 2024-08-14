using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_OCS.Models
{
    public class StoreProduct
    {
        public int Id { get; set; }

        public int? StoreId { get; set; }
        [ForeignKey("StoreId")]

        public Store? Store { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public StoreCategory? Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "available"; // 'available', 'pending', 'sold'
        public bool IsDeleted { get; set; } = false;
        public string Code { get; set; } = GenerateCode();

        private static string GenerateCode()
        {
            return $"ITEM-{DateTime.UtcNow.Ticks}-{new Random().Next(1000, 9999)}";
        }
    }
}
