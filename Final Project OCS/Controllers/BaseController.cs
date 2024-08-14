using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Final_Project_OCS.SD;
using Final_Project_OCS.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace Final_Project_OCS.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly ChatService _chatService;
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<IdentityUser> _userManager;


        public BaseController(ChatService chatService , ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _chatService = chatService;
            _context = context;
            _userManager = userManager;

        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ViewBag.ChatUsers = await _chatService.GetChatUsersAsync(User);

            await next();
        }

        public bool CheckNumberOfProduct()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return false;
            }

            var user = _context.ApplicationUsers
                .FirstOrDefault(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
            {
                return false;
            }

            

            return user.NumberOfAds >= user.NumberOfAdsAllowed;
        }




    }
}
