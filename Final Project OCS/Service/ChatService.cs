using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Final_Project_OCS.Service
{
    public class ChatService
    {
        protected readonly UserManager<IdentityUser> _userManager;
        protected readonly ApplicationDbContext _context;
        public ChatService(UserManager<IdentityUser> userManager , ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
            
        }

        public async Task<List<IdentityUser>> GetChatUsersAsync(ClaimsPrincipal user)
        {
            var currentUserId = _userManager.GetUserId(user);

            if (currentUserId == null)
            {
                throw new ArgumentNullException(nameof(user), "User is not authenticated.");
            }

            var userIds = await _context.ChatMessages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id) && u.Id != currentUserId)
                .ToListAsync();

            return users.ToList();
        }

    }
}
