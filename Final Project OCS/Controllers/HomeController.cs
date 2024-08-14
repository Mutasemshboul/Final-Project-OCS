using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
            var currentUserId = _userManager.GetUserId(User); // Get the logged-in user's ID

            // Trim the code to remove any extra spaces
            code = code?.Trim();

            // Search for the code in chat messages between the logged-in user and the product owner
            var exists = _context.ChatMessages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == ownerId) ||
                            (m.SenderId == ownerId && m.ReceiverId == currentUserId))
                .Any(m => m.Message.Contains(code));

            // Return the result as JSON
            return exists;
        }

        public void MarkAsSold(int productId)
        {
            var product = _context.Products.Find(productId);

            product.Status = "Sold";
            _context.SaveChangesAsync();


        }

    }
}
