namespace Final_Project_OCS.Models
{
    public class AdminDashboardViewModel
    {
        public int NumberOfUsers { get; set; }
        public int NumberOfProducts { get; set; }
        public int NumberOfCategories { get; set; }
        public int NumberOfStores { get; set; }
        public int NumberOfProductSwaps { get; set; }
        public double SoldProductsPercentage { get; set; }

        public int[] MonthlyNewUsers { get; set; } = new int[12];
        public int[] MonthlySoldProducts { get; set; } = new int[12];

        // New Properties for the Additional Charts
        public int SoldProducts { get; set; }
        public int AvailableProducts { get; set; }
        public decimal[] MonthlyRevenue { get; set; } = new decimal[12];
        public int[] MonthlyStoreGrowth { get; set; } = new int[12];
        public Dictionary<string, int> CategoryDistribution { get; set; } = new Dictionary<string, int>();

        // Recent activities
        public List<Product> RecentProducts { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; }
        public List<ProductSwap> RecentProductSwaps { get; set; }
        public List<Store> RecentStores { get; set; }
    }
}
