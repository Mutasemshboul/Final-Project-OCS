using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Microsoft.AspNetCore.SignalR;

namespace Final_Project_OCS.Chat
{
    public class ChatHub:Hub
    {
        private readonly ApplicationDbContext _context;
        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SendPrivateMessage(string receiverUserId, string message)
        {
            var senderUserId = Context.UserIdentifier;

            // Save the message to the database
            var chatMessage = new ChatMessage
            {
                SenderId = senderUserId,
                ReceiverId = receiverUserId,
                Message = message,
                Timestamp = DateTime.Now
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Send the message to the receiver
            await Clients.User(receiverUserId).SendAsync("ReceiveMessage", Context.User.Identity.Name, message);


        }
    }
}
