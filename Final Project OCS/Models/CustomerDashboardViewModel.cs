namespace Final_Project_OCS.Models
{
    public class CustomerDashboardViewModel
    {
        public int NumberOfProducts { get; set; }
        public int NumberOfStoreProducts { get; set; }
        public int NumberOfStoreCategories { get; set; }
        public int NumberOfSwapProduct { get; set; }
        public double SoldProductsPercentage { get; set; }

        public int[] MonthlySoldProducts { get; set; } = new int[12];
        public double[] MonthlyRevenue { get; set; } = new double[12];
        public int[] StoreGrowth { get; set; } = new int[12];
        public Dictionary<string, int> CategoryDistribution { get; set; }

        public List<Product> RecentProducts { get; set; }
        public List<ProductSwap> RecentSwapProducts { get; set; }
    }
}
