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
        public async Task<IActionResult> Details(int? id, bool isSwap = false,bool isProductStore = false)
        {


            Chat();

            if (id == null)
            {
                return NotFound();
            }
            if (isProductStore)
            {
                var product = await _context.StoreProducts
                   .Include(p => p.Store)
                   .FirstOrDefaultAsync(m => m.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                return View("Details", product);
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
        public JsonResult CheckCode(string code, string code2, string productOwnerId, int ProductId,string productType)
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
                    MarkAsSold(ProductId, productType);
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

        public void MarkAsSold(int productId , string productType)
        {
            if(productType == "product")
            {
                var product = _context.Products.Find(productId);
                var seller = _context.ApplicationUsers.Find(product.UserId);
                if (seller != null)
                {
                    seller.Points += 10;
                }

                var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var buyer = _context.ApplicationUsers.Find(buyerId);
                if (buyer != null)
                {
                    buyer.Points += 10;
                }

                product.Status = "Sold";
                product.SoldDate = DateTime.Now;
            }
            else if (productType == "swap")
            {
                var product = _context.ProductSwaps.Find(productId);
                var seller = _context.ApplicationUsers.Find(product.UserId);
                if (seller != null)
                {
                    seller.Points += 10;
                }

                var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var buyer = _context.ApplicationUsers.Find(buyerId);
                if (buyer != null)
                {
                    buyer.Points += 10;
                }

                product.Status = "Sold";
                product.SwapDate = DateTime.Now;
            }
            else if (productType == "store")
            {
                var product =  _context.StoreProducts
                    .Include(p => p.Store)
                    .FirstOrDefault(p => p.Id == productId);
                var seller = _context.ApplicationUsers.Find(product.Store.UserId);
                if (seller != null)
                {
                    seller.Points += 10;
                }

                var buyerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var buyer = _context.ApplicationUsers.Find(buyerId);
                if (buyer != null)
                {
                    buyer.Points += 10;
                }

                product.Status = "Sold";
            }
            

             _context.SaveChangesAsync();


        }

        public async Task<IActionResult> Subscriptions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.HasStore = _context.Stores.Any(s => s.UserId == userId);
            return View(await _context.SubscriptionTypes.ToListAsync());
        }
        [HttpPost]
        [Authorize]
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
        [Authorize]
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
       
        public IActionResult Products( bool isSwap)
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.IsSwap = isSwap;

            if (isSwap)
            {
                var products = _context.ProductSwaps
                    .Include(p => p.Category)
                    .Where(p=>p.Status == "Active")
                    .ToList();
                return View(products);
            }
            else
            {
                var products = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Status == "Active")
                    .ToList();
                return View(products);
            }
  
            
            
        }

        public IActionResult GetProductPartialView(int categoryId, bool isSwap)
        {
            ViewBag.IsSwap = isSwap;

            if (isSwap)
            {
                var products = _context.ProductSwaps
                           .Include(p => p.Category)
                           .Where(p => p.CategoryId == categoryId && p.Status == "Active")
                           .ToList();
                return PartialView("_ProductPartial", products);
            }
            else
            {
                var products = _context.Products
                           .Include(p => p.Category)
                           .Where(p => p.CategoryId == categoryId && p.Status == "Active")
                           .ToList();
                return PartialView("_ProductPartial", products);
            }
  
        }

        public async Task<IActionResult> Stores()
        {
            return View(await _context.Stores.ToListAsync());
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public async Task<IActionResult> CreateContactUs([Bind("Id,Name,Email,Subject,Body")] ContactUs contactUs)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contactUs);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contactUs);
        }

        public async Task<IActionResult> GetProductStore(int storeId)
        {
            
            var store = await _context.Stores.FindAsync(storeId);
            if (store == null)
            {
                return NotFound("Store not found");
            }

            var products = await _context.StoreProducts
                .Where(p => p.StoreId == storeId && p.Status=="Active")
                .Include(p => p.Category) 
                .ToListAsync();
            var categories = products.Select(p => p.Category).Distinct().ToList();

           
            ViewBag.Categories = categories;


            return View(products);
      
        }



    }
}
