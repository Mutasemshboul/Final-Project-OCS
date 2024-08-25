using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Final_Project_OCS.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                .Include(u=>u.User)
                    .OrderByDescending(ps => ps.CreationDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }


        public IActionResult Packages()
        {
            return View();
        }
        private async Task<double> CalculateSoldProductsPercentageAsync()
        {
            var totalProducts = await _context.Products.CountAsync();
            var soldProducts = await _context.Products.CountAsync(p => p.Status == "Sold");

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



    }
}
