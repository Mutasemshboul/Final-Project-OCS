using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_OCS.Service
{
    public class HasStoreAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HasStoreAttribute(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = _userManager.GetUserId(context.HttpContext.User);
            var hasStore = await _context.Stores.AnyAsync(s => s.UserId == userId);

            if (!hasStore)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
