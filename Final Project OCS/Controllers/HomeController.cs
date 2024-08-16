using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Final_Project_OCS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewBag.Products = _context.Products.Include(p => p.Category).Include(p => p.User).Where(p=>p.Status=="Active");
            ViewBag.ProductsSwap = _context.ProductSwaps.Include(p => p.Category).Include(p => p.User).Where(p => p.Status == "Active");
            return View();
        }

        [Authorize(Roles = SD.SD.Role_Customer)]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize]
        public async Task<IActionResult> Details(int? id, bool isSwap = false)
        {


            Chat();

            if (id == null)
            {
                return NotFound();
            }

            if (isSwap)
            {
                var productSwap = await _context.ProductSwaps
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (productSwap == null)
                {
                    return NotFound();
                }

                return View("Details", productSwap);
            }
            else
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                return View("Details", product);
            }
        }
        public  void Chat()
        {

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                // Log or handle the error
                throw new Exception("Current user ID is null or empty.");
            }

            var users = _userManager.Users.Where(u => u.Id != currentUserId).ToList();
            var messages =  _context.ChatMessages
           .Include(m => m.Sender)
           .Include(m => m.Receiver)
           .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
           .OrderBy(m => m.Timestamp)
           .ToList();

            var viewModel = new ChatViewModel
            {
                Users = users,
                Messages = messages,
                CurrentUserId = currentUserId
            };
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ChatViewModel = viewModel;
            
        }
        public async Task<IActionResult> GetChatWithUser(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
            ViewBag.CurrentUserId = currentUserId;
            return PartialView("_ChatMessage", messages);
        }
        [HttpGet]
        public JsonResult CheckCode(string code, string code2, string productOwnerId, int ProductId)
        {
            bool exists;
            string message;

            code = code?.Trim();

            bool isCodeInChat = SearchCodeInChat(code, productOwnerId);

            if (isCodeInChat)
            {
                if (code == code2)
                {
                    exists = true;
                    message = "Code found in chat and it matches the provided code!";
                    MarkAsSold(ProductId);
                }
                else
                {
                    exists = false;
                    message = "Code found in chat but does not match the provided code.";
                }
            }
            else
            {
                exists = false;
                message = "Code not found in chat.";
            }

            return Json(new { exists, message });
        }


        public bool SearchCodeInChat(string code, string ownerId)
        {
            var currentUserId = _userManager.GetUserId(User); 

            code = code?.Trim();

            var exists = _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == ownerId) ||
                            (m.SenderId == ownerId && m.ReceiverId == currentUserId))
                .Any(m => m.Message.Contains(code));

            return exists;
        }

        public void MarkAsSold(int productId)
        {
            var product = _context.Products.Find(productId);

            product.Status = "Sold";
            _context.SaveChangesAsync();


        }

        public async Task<IActionResult> Subscriptions()
        {
            return View(await _context.SubscriptionTypes.ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> AddSubscription(int subscriptionTypeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var subscriptionType = await _context.SubscriptionTypes.FindAsync(subscriptionTypeId);
            if (subscriptionType == null)
            {
                return Json(new { success = false, message = "Invalid subscription type." });
            }

            var newSubscription = new Subscription
            {
                UserId = userId,
                SubscriptionTypeId = subscriptionTypeId,
                
            };

            _context.Subscriptions.Add(newSubscription);

            var user = await _context.ApplicationUsers.FindAsync(userId);
            if (user != null)
            {
                user.NumberOfAdsAllowed += subscriptionType.NumberOfAdsAllowed;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Subscription added successfully. Your ad limit has been updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStore(Store store)
        {
            if (ModelState.IsValid)
            {
                store.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.ApplicationUsers
                .FirstOrDefault(u => u.Id == store.UserId && !u.IsDeleted);
                user.HasStore = true;
                _context.ApplicationUsers.Update(user);
                _context.Stores.Add(store);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Store registered successfully." });
            }

            return Json(new { success = false, message = "There was an error registering the store. Please try again." });
        }
       
        public IActionResult Products(int categoryId)
        {
            ViewBag.Categories = _context.Categories.ToList();
            IEnumerable<Product> products;

            if ( categoryId > 0)
            {
                // If a category is specified, get products from that category
                products = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId)
                    .ToList();
            }
            else
            {
                // If no category is specified, get all products
                products = _context.Products
                    .Include(p => p.Category)
                    .ToList();
            }

           
                PartialView("_ProductPartial", products);
            
            return View();
            
        }



    }
}
