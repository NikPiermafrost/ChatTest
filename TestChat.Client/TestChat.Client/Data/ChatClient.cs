using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestChat.Client.Data
{
    public class ChatClient: IAsyncDisposable
    {
        public const string HubUrl = "/ChatHub";
        private readonly NavigationManager _navigationManager;
        private HubConnection _hubConnection;
        private readonly string _userName;
        private bool _started = false;

        public ChatClient(NavigationManager navigationManager, string userName)
        {
            _navigationManager = navigationManager;
            _userName = userName;
        }

        public async Task StartAsync()
        {
            if (!_started)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navigationManager.ToAbsoluteUri(HubUrl))
                    .Build();

                Console.WriteLine("Chatclient:calling Start()");

                _hubConnection.On<string, string>(Messages.Receive, (user, message) => 
                {
                    HandleReceiveMessage(user, message);
                });

                await _hubConnection.StartAsync();

                Console.WriteLine("Chatclient:Start returned");
                _started = true;

                await _hubConnection.SendAsync(Messages.Register, _userName);
            }
        }

        private void HandleReceiveMessage(string user, string message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(user, message));
        }

        public event MessageReceivedEventHandler MessageReceived;
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

        public async Task TaskAsync(string message)
        {
            if (!_started)
            {
                throw new InvalidOperationException("Client not started");
            }
            await _hubConnection.SendAsync(Messages.Send, _userName, message);
        }

        public async Task StopAsync()
        {
            if (_started)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
                _started = false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("ChatClient:Disposing");
            await StopAsync();
        }
    }

    public class MessageReceivedEventArgs: EventArgs
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public MessageReceivedEventArgs(string username, string message)
        {
            Username = username;
            Message = message;
        }
    }
}
