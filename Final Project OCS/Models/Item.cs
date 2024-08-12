﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_OCS.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        [ForeignKey("UserId")]

        public ApplicationUser? User { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category? Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        
        public string Status { get; set; } = "available"; // 'available', 'pending', 'sold'
        public bool IsDeleted { get; set; } = false;
    }
}
