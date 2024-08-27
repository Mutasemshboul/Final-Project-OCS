using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Final_Project_OCS.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Final_Project_OCS.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext context, ChatService chatService): base(chatService, context, userManager)
        {
            
        }
        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                var viewModel = new AdminDashboardViewModel
                {
                    NumberOfUsers = await _context.Users.CountAsync(),
                    NumberOfProducts = await _context.Products.CountAsync(),
                    NumberOfCategories = await _context.Categories.CountAsync(),
                    NumberOfStores = await _context.Stores.CountAsync(),
                    NumberOfProductSwaps = await _context.ProductSwaps.CountAsync(),

                    SoldProducts = await _context.Products.CountAsync(p => p.Status == "Sold"),
                    AvailableProducts = await _context.Products.CountAsync(p => p.Status == "Active"),
                    SoldProductsPercentage = await CalculateSoldProductsPercentageAsync(),
                    MonthlyNewUsers = await GetMonthlyNewUsersAsync(),
                    MonthlySoldProducts = await GetMonthlySoldProductsAsync(),
                    MonthlyRevenue = await GetMonthlyRevenueAsync(),
                    MonthlyStoreGrowth = await GetMonthlyStoreGrowthAsync(),
                    CategoryDistribution = await GetCategoryDistributionAsync(),

                    RecentProducts = await _context.Products
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .ToListAsync(),
                    RecentUsers = await _context.ApplicationUsers
                    .OrderByDescending(u => u.RegistrationDate)
                    .Take(5)
                    .ToListAsync(),
                    RecentProductSwaps = await _context.ProductSwaps
                    .OrderByDescending(ps => ps.SwapDate)
                    .Take(5)
                    .ToListAsync(),
                    RecentStores = await _context.Stores
                .Include(u => u.User)
                    .OrderByDescending(ps => ps.CreationDate)
                    .Take(5)
                    .ToListAsync()
                };

                return View(viewModel);
            }
            else
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customerDashboardViewModel = new CustomerDashboardViewModel
                {
                    NumberOfProducts = await _context.Products.Where(p=>!p.IsDeleted).CountAsync(p => p.UserId == customerId),
                    NumberOfStoreProducts = await _context.StoreProducts.Where(p => !p.IsDeleted).CountAsync(sp => sp.Store.UserId == customerId),
                    NumberOfStoreCategories = await _context.StoreCategories.Where(p => !p.IsDeleted).CountAsync(sc => sc.Store.UserId == customerId),
                    NumberOfSwapProduct = await _context.ProductSwaps.Where(p => !p.IsDeleted).CountAsync(sp => sp.UserId == customerId),
                    SoldProductsPercentage = await CalculateCustomerSoldProductsPercentageAsync(customerId),
                    MonthlySoldProducts = await GetCustomerMonthlySoldProductsAsync(customerId),
                    MonthlyRevenue = await GetCustomerMonthlyRevenueAsync(customerId),
                    StoreGrowth = await GetCustomerStoreGrowthAsync(customerId),
                    CategoryDistribution = await GetCustomerCategoryDistributionAsync(customerId),
                    RecentProducts = await GetRecentCustomerProductsAsync(customerId),
                    RecentSwapProducts = await _context.ProductSwaps
                    .OrderByDescending(ps => ps.SwapDate)
                    .Where(u=>u.UserId == customerId && !u.IsDeleted)
                    .Take(5)
                    .ToListAsync()
                };
                return View("CustomerDashboard", customerDashboardViewModel);
            }
            
                
        }


        public IActionResult Packages()
        {
            return View();
        }
        private async Task<double> CalculateSoldProductsPercentageAsync()
        {
            var totalProducts = await _context.Products.Where(p => !p.IsDeleted).CountAsync();
            var soldProducts = await _context.Products.Where(p => !p.IsDeleted).CountAsync(p => p.Status == "Sold");

            if (totalProducts == 0) return 0;

            return (double)soldProducts / totalProducts * 100;
        }




        private async Task<int[]> GetMonthlyNewUsersAsync()
        {
            var monthlyNewUsers = new int[12];
            var currentYear = DateTime.Now.Year;

            for (int i = 1; i <= 12; i++)
            {
                monthlyNewUsers[i - 1] = await _context.ApplicationUsers
                    .Where(u => u.RegistrationDate.Year == currentYear && u.RegistrationDate.Month == i)
                    .CountAsync();
            }

            return monthlyNewUsers;
        }


        private async Task<int[]> GetMonthlySoldProductsAsync()
        {
            var monthlySoldProducts = new int[12];
            var currentYear = DateTime.Now.Year;

            for (int i = 1; i <= 12; i++)
            {
                monthlySoldProducts[i - 1] = await _context.Products
                    .Where(p =>  p.SoldDate.Year == currentYear && p.SoldDate.Month == i)
                    .CountAsync();
            }

            return monthlySoldProducts;
        }
        private async Task<decimal[]> GetMonthlyRevenueAsync()
        {
            var monthlyRevenue = new decimal[12];
            var currentYear = DateTime.Now.Year;

            for (int i = 1; i <= 12; i++)
            {
                monthlyRevenue[i - 1] = await _context.Products
                    .Where(p =>  p.SoldDate.Year == currentYear && p.SoldDate.Month == i)
                    .SumAsync(p => p.Price);
            }

            return monthlyRevenue;
        }

        private async Task<int[]> GetMonthlyStoreGrowthAsync()
        {
            var monthlyStoreGrowth = new int[12];
            var currentYear = DateTime.Now.Year;

            for (int i = 1; i <= 12; i++)
            {
                monthlyStoreGrowth[i - 1] = await _context.Stores
                    .Where(s => s.CreationDate.Year == currentYear && s.CreationDate.Month == i)
                    .CountAsync();
            }

            return monthlyStoreGrowth;
        }

        private async Task<Dictionary<string, int>> GetCategoryDistributionAsync()
        {
            var categoryDistribution = await _context.Products
                .GroupBy(p => p.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Category, g => g.Count);

            return categoryDistribution;
        }

        private async Task<double> CalculateCustomerSoldProductsPercentageAsync(string customerId)
        {
            var totalProducts = await _context.Products.CountAsync(p => p.UserId == customerId);
            if (totalProducts == 0) return 0;

            var soldProducts = await _context.Products.CountAsync(p => p.UserId == customerId && p.Status == "Sold");
            return (double)soldProducts / totalProducts * 100;
        }

        private async Task<int[]> GetCustomerMonthlySoldProductsAsync(string customerId)
        {
            int[] monthlySoldProducts = new int[12];
            var soldProducts = await _context.Products
                .Where(p => p.UserId == customerId && p.Status == "Sold")
                .GroupBy(p => p.SoldDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in soldProducts)
            {
                monthlySoldProducts[item.Month - 1] = item.Count;
            }

            return monthlySoldProducts;
        }

        private async Task<double[]> GetCustomerMonthlyRevenueAsync(string customerId)
        {
            double[] monthlyRevenue = new double[12];
            var revenue = await _context.Products
                .Where(p => p.UserId == customerId && p.Status == "Sold")
                .GroupBy(p => p.SoldDate.Month)
                .Select(g => new { Month = g.Key, TotalRevenue = g.Sum(p => p.Price) })
                .ToListAsync();

            foreach (var item in revenue)
            {
                monthlyRevenue[item.Month - 1] = (double)item.TotalRevenue;
            }

            return monthlyRevenue;
        }
        private async Task<int[]> GetCustomerStoreGrowthAsync(string customerId)
        {
            int[] storeGrowth = new int[12];
            var storeGrowthData = await _context.Stores
                .Where(s => s.UserId == customerId)
                .GroupBy(s => s.CreationDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in storeGrowthData)
            {
                storeGrowth[item.Month - 1] = item.Count;
            }

            return storeGrowth;
        }

        private async Task<Dictionary<string, int>> GetCustomerCategoryDistributionAsync(string customerId)
        {
            var categoryDistribution = await _context.StoreCategories
                .Where(c => c.Store.UserId == customerId)
                .GroupBy(c => c.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CategoryName, g => g.Count);

            return categoryDistribution;
        }

        private async Task<List<Product>> GetRecentCustomerProductsAsync(string customerId)
        {
            return await _context.Products
                .Where(p => p.UserId == customerId)
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToListAsync();
        }

    }
}
