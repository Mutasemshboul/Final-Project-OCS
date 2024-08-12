using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_OCS.Models
{
    public class ImageItem
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        [ValidateNever]
        public Item? Item { get; set; }
    }
}
