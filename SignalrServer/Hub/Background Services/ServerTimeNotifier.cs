using Microsoft.AspNetCore.SignalR;
using SignalrServer.Hub;

namespace SignalrServer
{
    public class ServerTimeNotifier : BackgroundService
    {
        private static readonly TimeSpan Period = TimeSpan.FromSeconds(5);
        private readonly ILogger<ServerTimeNotifier> _logger;
        private readonly IHubContext<NotificationsHub,INotificationClient> _context;

        public ServerTimeNotifier(ILogger<ServerTimeNotifier> logger, IHubContext<NotificationsHub, INotificationClient> context)
        {
            _logger = logger;
            _context = context;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Period);

            while (!stoppingToken.IsCancellationRequested && 
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                var dateTime = DateTime.Now;

                _logger.LogInformation("Executing {Service} {Time}",nameof(ServerTimeNotifier),dateTime);

                await _context.Clients.All.ReceiveNotification($"Server time = {dateTime}");
                //await _context.Clients.User("f45fe475-8466-484f-af69-a2658a8ee915").ReceiveNotification($"Server time = {dateTime}");
                  
            }
        }
    }
}

/*
  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImY0NWZlNDc1LTg0NjYtNDg0Zi1hZjY5LWEyNjU4YThlZTkxNSIsInN1YiI6ImY0NWZlNDc1LTg0NjYtNDg0Zi1hZjY5LWEyNjU4YThlZTkxNSIsImp0aSI6ImRjZmU2ZWRkIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6MzI0ODYiLCJodHRwczovL2xvY2FsaG9zdDo0NDM5NiIsImh0dHA6Ly9sb2NhbGhvc3Q6NTE4MiIsImh0dHBzOi8vbG9jYWxob3N0OjcyMTgiXSwibmJmIjoxNzAzMDc4MTk5LCJleHAiOjE3MTA5NDA1OTksImlhdCI6MTcwMzA3ODIwMSwiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.GTySfBkxSNS1925EtbmtiPzbcgkh4Bd2VuhHZn86Tbs
 */