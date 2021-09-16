using APIServices.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APIServices.Services.HubSignalR
{
    public class ChatHub : Hub
    {
        private readonly IOptions<AppSettings> _appSettings;
        public ChatHub(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        public override Task OnConnectedAsync()
        {
            UserService userService = new UserService(_appSettings);

            string name = Context.User.Identity.Name;
            userService.UpdateConnectionId(1, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(string user, string message)
        {
            string name = Context.User.Identity.Name;

            await Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId, message);
        }
    }
}