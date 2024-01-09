using Backgammon_DLL_v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalrServer.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SignalrServer.Hub
{
    [Authorize(Roles = "Admin,User")]
    public class NotificationsHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Client(Context.ConnectionId).ReceiveNotification(
                $"{Context.User?.Identity?.Name} you are connected");

            await base.OnConnectedAsync();
        }
        public async Task FindUserAsync(string username)
        {
            string message = "Error";
            try
            {
                var user = Context.User?.FindFirst(u => u.Value == username);
                if (user != null)
                {
                    message = user.Value;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            await Clients.Caller.ReceiveNotification(message);
        }
        public async Task Test_1(MessageModel message)
        {
           
            var json = JsonSerializer.Serialize( message);

            //var content = new StringContent(json, Encoding.UTF8, "application/json");
            await Clients.Caller.ReceiveMessage(json);
        }

        public async Task Test_2(string group, string message)
        {
            await Clients.Group(group).ReceiveNotification($"{message}\nfrom {group} group");
        }
        public async Task JoinGroup(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId,group);
        }

    }

    public interface INotificationClient
    {
        Task ReceiveNotification(string message);
        Task ReceiveMessage(string message);
    }
}
