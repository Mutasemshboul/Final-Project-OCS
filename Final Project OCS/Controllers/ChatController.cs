using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_OCS.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Chat()
        {

            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                // Log or handle the error
                throw new Exception("Current user ID is null or empty.");
            }

            var users = _userManager.Users.Where(u => u.Id != currentUserId).ToList();
            var messages = await _context.ChatMessages
           .Include(m => m.Sender)
           .Include(m => m.Receiver)
           .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
           .OrderBy(m => m.Timestamp)
           .ToListAsync();

            var viewModel = new ChatViewModel
            {
                Users = users,
                Messages = messages,
                CurrentUserId = currentUserId
            };
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ChatViewModel = viewModel;
            return View();
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
            return PartialView("_ChatHistory", messages);
        }
    }
}
