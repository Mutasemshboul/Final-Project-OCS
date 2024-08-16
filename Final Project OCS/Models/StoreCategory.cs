using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_OCS.Models
{
    public class StoreCategory
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        public string Description { get; set; }
        public int? StoreId { get; set; }
        [ForeignKey("StoreId")]

        public Store? Store { get; set; }
    }
}
