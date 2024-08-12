using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Final_Project_OCS.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Final_Project_OCS.Controllers
{
    public class BaseController : Controller
    {
        private readonly ChatService _chatService;

        public BaseController(ChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Fetch chat users once and store in ViewBag
            ViewBag.ChatUsers = await _chatService.GetChatUsersAsync(User);

            // Continue with the execution of the action
            await next();
        }
    }
}
