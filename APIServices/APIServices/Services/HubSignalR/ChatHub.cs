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

        public async Task SendMessage(string user, string message)
        {
            UserService userService = new UserService(_appSettings);
            await Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId, message);
        }
    }
}