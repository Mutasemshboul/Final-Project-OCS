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
        protected readonly UserManager<IdentityUser> _userManager;
        protected readonly ApplicationDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext context, ChatService chatService): base(chatService)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            return View();
        }
        
    }
}
