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
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.Status == "Active" && !p.IsDeleted && !p.Category.IsDeleted && !p.User.IsDeleted)
                .ToList();

            var productSwaps = _context.ProductSwaps
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.Status == "Active" && !p.IsDeleted && !p.Category.IsDeleted && !p.User.IsDeleted)
                .ToList();

            var productsWithImages = products.Select(p => new
            {
                Product = p,
                FirstImageUrl = _context.ImageItems
                    .Where(pi => pi.ItemId == p.Id)
                    .OrderBy(pi => pi.Id) 
                    .Select(pi => pi.ImageUrl)
                    .FirstOrDefault()
            }).ToList();

            var productSwapsWithImages = productSwaps.Select(ps => new
            {
                ProductSwap = ps,
                FirstImageUrl = _context.ImageItems
                    .Where(psi => psi.ItemId == ps.Id)
                    .OrderBy(psi => psi.Id) 
                    .Select(psi => psi.ImageUrl)
                    .FirstOrDefault()
            }).ToList();

            ViewBag.Products = productsWithImages;
            ViewBag.ProductSwaps = productSwapsWithImages;

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
                    .Include(c=>c.Category)
                   .Include(p => p.Store)
                   .FirstOrDefaultAsync(m => m.Id == id);
                if (product == null)
                {
                    return NotFound();
                }
                var productImages = _context.StoreProductImages
                                 .Where(sp => sp.ItemId == id)
                                 .ToList();
                ViewBag.ProductsImages = productImages;
                if (productImages.Any())
                {
                    ViewBag.FirstImage = productImages.First().ImageUrl;
                }
                else
                {
                    ViewBag.FirstImage = "path-to-default-image"; 
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
                var productImages = _context.ImageItems
                    .Where(sp => sp.ItemId == id)
                    .ToList();
                ViewBag.ProductsImages = productImages;
                if (productImages.Any())
                {
                    ViewBag.FirstImage = productImages.First().ImageUrl;
                }
                else
                {
                    ViewBag.FirstImage = "path-to-default-image";
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
                var productImages = _context.ImageItems
                    .Where(sp => sp.ItemId == id)
                    .ToList();
                ViewBag.ProductsImages = productImages;
                if (productImages.Any())
                {
                    ViewBag.FirstImage = productImages.First().ImageUrl;
                }
                else
                {
                    ViewBag.FirstImage = "path-to-default-image";
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
                product.SoldDate = DateTime.Now;
            }
            

             _context.SaveChangesAsync();


        }

        public async Task<IActionResult> Subscriptions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.HasStore = _context.Stores.Any(s => s.UserId == userId);
            return View(await _context.SubscriptionTypes.Where(su=>!su.IsDeleted).ToListAsync());
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
        [HttpPost]
        public async Task<IActionResult> RegisterStore(Store store)
        {
            if (ModelState.IsValid)
            {
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString()+ Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/stores", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        store.ImageUrl = "/images/stores/" + fileName;
                    }
                }

                store.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.ApplicationUsers
                    .FirstOrDefault(u => u.Id == store.UserId && !u.IsDeleted);
                if (user != null)
                {
                    user.HasStore = true;
                    _context.ApplicationUsers.Update(user);
                    _context.Stores.Add(store);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Store registered successfully." });
                }
            }

            return Json(new { success = false, message = "There was an error registering the store. Please try again." });
        }


        public IActionResult Products(bool isSwap)
        {
            // Load categories for the ViewBag
            ViewBag.Categories = _context.Categories.Where(c => !c.IsDeleted).ToList();
            ViewBag.IsSwap = isSwap;

            if (isSwap)
            {
                var products = _context.ProductSwaps
                    .Include(p => p.Category)
                    .Include(u => u.User)
                    .Where(p => p.Status == "Active" && !p.IsDeleted && !p.User.IsDeleted && !p.Category.IsDeleted)
                    .Select(ps => new
                    {
                        Type = "ProductSwap",
                        ProductSwap = ps,
                        FirstImageUrl = _context.ImageItems
                            .Where(psi => psi.ItemId == ps.Id)
                            .OrderBy(psi => psi.Id)
                            .Select(psi => psi.ImageUrl)
                            .FirstOrDefault()
                    })
                    .ToList<dynamic>(); 

                return View(products);
            }
            else
            {
                var products = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Status == "Active" && !p.IsDeleted && !p.User.IsDeleted && !p.Category.IsDeleted)
                    .Select(p => new
                    {
                        Type = "Product",
                        Product = p,
                        FirstImageUrl = _context.ImageItems
                            .Where(pi => pi.ItemId == p.Id)
                            .OrderBy(pi => pi.Id)
                            .Select(pi => pi.ImageUrl)
                            .FirstOrDefault()
                    })
                    .ToList<dynamic>(); 

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
                    .Include(u => u.User)
                    .Where(p => p.Status == "Active" && !p.IsDeleted && !p.User.IsDeleted && !p.Category.IsDeleted && p.CategoryId == categoryId)
                    .Select(ps => new
                    {
                        Type ="ProductSwap" ,
                        ProductSwap = ps,
                        FirstImageUrl = _context.ImageItems
                            .Where(psi => psi.ItemId == ps.Id)
                            .OrderBy(psi => psi.Id)
                            .Select(psi => psi.ImageUrl)
                            .FirstOrDefault()
                    })
                    .ToList<dynamic>(); 

                return PartialView("_ProductPartial", products);
            }
            else
            {
                var products = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Status == "Active" && !p.IsDeleted && !p.User.IsDeleted && !p.Category.IsDeleted && p.CategoryId == categoryId)
                    .Select(p => new
                    {
                        Type = "Product",
                        Product = p,
                        FirstImageUrl = _context.ImageItems
                            .Where(pi => pi.ItemId == p.Id)
                            .OrderBy(pi => pi.Id)
                            .Select(pi => pi.ImageUrl)
                            .FirstOrDefault()
                    })
                    .ToList<dynamic>(); 

                return PartialView("_ProductPartial", products);
            }

        }

        public async Task<IActionResult> Stores()
        {
            return View(await _context.Stores.Where(s=>!s.IsDeleted).ToListAsync());
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
                .Include(p => p.Category)
                .Include(p => p.Category.Store.User)
                .Where(p => p.Status == "Active" && !p.IsDeleted && !p.Category.Store.User.IsDeleted && !p.Category.IsDeleted && p.StoreId == storeId)
                .Select(p => new
                {
                    Product = p,
                    FirstImageUrl = _context.StoreProductImages
                        .Where(pi => pi.ItemId == p.Id)
                        .OrderBy(pi => pi.Id) // Or use CreatedDate if available
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var categories = products.Select(p => p.Product.Category).Distinct().ToList();

            ViewBag.Categories = categories;
            ViewBag.Products = products;

            return View(products);
        }




    }
}
