namespace Final_Project_OCS.Models
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public int SubscriptionTypeId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ApplicationUser User { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
    }
}
