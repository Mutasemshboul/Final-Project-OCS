namespace Final_Project_OCS.Models
{
    public class SubscriptionType
    {
        public int SubscriptionTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int NumberOfAdsAllowed { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
