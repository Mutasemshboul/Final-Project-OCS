using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_OCS.Models
{
    public class Product:Item
    {
        [Required]
        public decimal Price { get; set; }
        public DateTime SoldDate { get; set; }
    }
}
