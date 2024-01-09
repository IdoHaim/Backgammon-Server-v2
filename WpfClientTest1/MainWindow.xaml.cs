using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using WpfClientTest1.Models;
using Backgammon_DLL_v1;

namespace WpfClientTest1
{
    public partial class MainWindow : Window
    {
        const string pathAuth = "https://localhost:7218/api/Auth";
        HubConnection connection;
        static string jwtToken = String.Empty;

        UserDto userDto = new UserDto
        {
            UserName = "example_user",
            Password = "example_password"
        };

        public MainWindow()
        {
            InitializeComponent();


        }
        private async Task RegisterAsync()
        {
            string fullpath = pathAuth + "/register";

            // Serialize UserDto object to JSON
            var json = JsonSerializer.Serialize(userDto);

            using var httpClient = new HttpClient();

            // Create a StringContent with JSON data
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Send POST request
                var response = await httpClient.PostAsync(fullpath, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Process the response if needed
                    this.Dispatcher.Invoke(() =>
                    {
                        messages.Items.Add("Registration successful!");
                        messages.Items.Add(responseContent);
                    });

                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        messages.Items.Add("Failed to register. Status code: " + response.StatusCode);
                    });
                }
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }

        }

        private async Task LoginAsync()
        {
            string fullpath = pathAuth + "/login";

            // Serialize UserDto object to JSON
            var json = JsonSerializer.Serialize(userDto);

            using var httpClient = new HttpClient();

            // Create a StringContent with JSON data
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Send POST request
                var response = await httpClient.PostAsync(fullpath, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    jwtToken = responseContent;
                    // Process the response if needed
                    this.Dispatcher.Invoke(() =>
                    {
                        messages.Items.Add("Logged in successfully!");
                        messages.Items.Add(responseContent);
                    });

                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        messages.Items.Add("Failed to login. Status code: " + response.StatusCode);
                    });
                }
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }


        private async void openConnection_Click(object sender, RoutedEventArgs e)
        {
            BuildConnection();

            connection.On<string>("ReceiveNotification", message =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    messages.Items.Add(message);
                });

            });
            connection.On<string>(ClientSideChannels.ReceiveMessage.ToString(), message =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MessageModel serializedMessage = JsonSerializer.Deserialize<MessageModel>(message)!;
                    if (serializedMessage != null)
                    {
                        messages.Items.Add("Sender: " + serializedMessage.Sender);
                        messages.Items.Add("Receiver: " + serializedMessage.Receiver);
                        messages.Items.Add("Message: " + serializedMessage.Message);

                    }

                });

            });

            try
            {
                await connection.StartAsync();
                messages.Items.Add("Connection started");
                openConnection.IsEnabled = false;
                sendMessage.IsEnabled = true;
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }
        private void BuildConnection()
        {
            connection = new HubConnectionBuilder()
               .WithUrl("https://localhost:7218/notifications",
               o => o.AccessTokenProvider = () => Task.FromResult<string?>(
               jwtToken))
               .WithAutomaticReconnect()
               .Build();

            connection.Reconnecting += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Attempting to reconnect...";
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };
            connection.Reconnected += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Reconnect to the server";
                    messages.Items.Clear();
                    messages.Items.Add(newMessage);
                });

                return Task.CompletedTask;
            };
            connection.Closed += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Connection closed";
                    messages.Items.Add(newMessage);
                    openConnection.IsEnabled = true;
                    sendMessage.IsEnabled = false;
                });

                return Task.CompletedTask;
            };
        }
        private void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            //await connection.InvokeAsync("FindUserAsync", messageInput.Text);
            //Test_1();
            //Test_2();
        }
        private async void Test_1()
        {
            MessageModel message = new MessageModel("user1", "user2", "hello");
            await connection.InvokeAsync("Test_1", message);
        }
        private async void Test_2()
        {
            await connection.InvokeAsync("JoinGroup", "group1");
            await connection.InvokeAsync("Test_2", "group1","private group message");
        }

        private async void registerBTN_Click(object sender, RoutedEventArgs e)
        {
            await RegisterAsync();
            await LoginAsync();
        }

        private void userNameBTN_Click(object sender, RoutedEventArgs e)
        {
            userDto.UserName = messageInput.Text;
        }
    }
}



//previus hard coded jwt token:
//"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImY0NWZlNDc1LTg0NjYtNDg0Zi1hZjY5LWEyNjU4YThlZTkxNSIsInN1YiI6ImY0NWZlNDc1LTg0NjYtNDg0Zi1hZjY5LWEyNjU4YThlZTkxNSIsImp0aSI6ImRjZmU2ZWRkIiwiYXVkIjpbImh0dHA6Ly9sb2NhbGhvc3Q6MzI0ODYiLCJodHRwczovL2xvY2FsaG9zdDo0NDM5NiIsImh0dHA6Ly9sb2NhbGhvc3Q6NTE4MiIsImh0dHBzOi8vbG9jYWxob3N0OjcyMTgiXSwibmJmIjoxNzAzMDc4MTk5LCJleHAiOjE3MTA5NDA1OTksImlhdCI6MTcwMzA3ODIwMSwiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.GTySfBkxSNS1925EtbmtiPzbcgkh4Bd2VuhHZn86Tbs"