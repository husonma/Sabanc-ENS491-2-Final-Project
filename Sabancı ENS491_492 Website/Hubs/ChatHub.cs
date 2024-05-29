using Microsoft.EntityFrameworkCore;
using Sabancı_ENS491_492_Website.Models;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Sabancı_ENS491_492_Website.Data;

namespace Sabancı_ENS491_492_Website.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ProjectContext _context;

        public ChatHub(ProjectContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            if (int.TryParse(senderId, out int senderIdInt) && int.TryParse(receiverId, out int receiverIdInt))
            {
                var sender = await _context.Users.FindAsync(senderIdInt);
                var receiver = await _context.Users.FindAsync(receiverIdInt);

                if (sender != null && receiver != null)
                {
                    var chatMessage = new ChatMessage
                    {
                        SenderId = senderIdInt,
                        ReceiverId = receiverIdInt,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    };

                    _context.ChatMessages.Add(chatMessage);
                    await _context.SaveChangesAsync();

                    // Send message to the specific receiver
                    await Clients.User(receiverId).SendAsync("ReceiveMessage", sender.Name, message, chatMessage.Timestamp);
                    // Send message back to the sender's client
                    await Clients.Caller.SendAsync("ReceiveMessage", "You", message, chatMessage.Timestamp);
                }
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
