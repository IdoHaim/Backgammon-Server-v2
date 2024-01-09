namespace SignalrServer.Models
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string ConnectionId { get; set; }
        public UserInfo(string userName,string connectionId)
        {
            UserName = userName;
            ConnectionId = connectionId;
        }
    }
}
