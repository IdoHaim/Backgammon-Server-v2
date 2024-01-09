using Backgammon_DLL_v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SignalrServer.Models;
using System.Collections.Concurrent;

namespace SignalrServer.Hub
{
    [Authorize(Roles = "Admin,User")]
    public class MainHub : Hub<IClientActions> ,IServerActions
    {
        private readonly UsersContext _usersContext;
        //private static readonly List<UserInfo> OnlineUsers = new List<UserInfo>();
        private static readonly ConcurrentDictionary<string,string> OnlineUsers = new ConcurrentDictionary<string,string>();


        public MainHub(UsersContext usersContext)
        {
            _usersContext = usersContext;
        }

        /// <summary>
        /// connect and disconnect
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.Name != null)
            {
               bool isSeccesfull = OnlineUsers.TryAdd(Context.User.Identity.Name,Context.ConnectionId);
               
            }
            UpdateAndSendLists();

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User?.Identity?.Name != null)
                OnlineUsers.Remove(Context.User.Identity.Name,out var ignore);
            UpdateAndSendLists();
            
            return base.OnDisconnectedAsync(exception);
        }


        /// <summary>
        /// messages
        /// </summary>      
        public Task SendMessage(MessageModel message)
        {
            try
            {
                return Clients.Client(OnlineUsers[message.Receiver]).ReceiveMessage(message);   
            }
            catch (Exception ex)
            {
                return Clients.Caller.Error(ex.Message);
            }

        }


        /// <summary>
        /// Inner Actions
        /// </summary>
        private void UpdateAndSendLists()
        {
            var offlineUsers = _usersContext.Users.Select(u => u.UserName).Except(OnlineUsers.Keys);

            string onlineUsersJson = JsonConvert.SerializeObject(OnlineUsers);
            string offlineUsersJson = JsonConvert.SerializeObject(offlineUsers);

            Clients.All.ReceiveUsersLists(onlineUsersJson, offlineUsersJson);
        }

    }
}
