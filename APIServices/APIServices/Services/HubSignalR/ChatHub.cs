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
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        //private IMemoryCache _cache;
        //private IHubContext<ChatHub> _hubContext;
        //private readonly AppSettings _appSettings;
        //public ChatHub(IMemoryCache cache, IHubContext<ChatHub> hubContext, IOptions<AppSettings> appSettings)
        //{
        //    _cache = cache;
        //    _hubContext = hubContext;
        //    _appSettings = appSettings.Value;
        //}

        //public async Task SendMessage()
        //{
        //    if (!_cache.TryGetValue("SpeedAlarm", out string response))
        //    {
        //        ListenForAlarmNotifications();
        //        string jsonspeedalarm = GetAlarmList();
        //        _cache.Set("SpeedAlarm", jsonspeedalarm);
        //        await Clients.All.SendAsync("ReceiveMessage", _cache.Get("SpeedAlarm").ToString());
        //    }
        //    else
        //    {
        //        await Clients.All.SendAsync("ReceiveMessage", _cache.Get("SpeedAlarm").ToString());
        //    }
        //}

        //public void ListenForAlarmNotifications()
        //{
        //    NpgsqlConnection conn = new NpgsqlConnection(_appSettings.ConnectionString);
        //    conn.StateChange += conn_StateChange;
        //    conn.Open();
        //    var listenCommand = conn.CreateCommand();
        //    listenCommand.CommandText = $"listen notifyalarmspeed;";
        //    listenCommand.ExecuteNonQuery();
        //    conn.Notification += PostgresNotificationReceived;
        //    _hubContext.Clients.All.SendAsync(this.GetAlarmList());
        //    while (true)
        //    {
        //        conn.Wait();
        //    }
        //}
        //private void PostgresNotificationReceived(object sender, NpgsqlNotificationEventArgs e)
        //{

        //    string actionName = e.Payload.ToString();
        //    string actionType = "";
        //    if (actionName.Contains("DELETE"))
        //    {
        //        actionType = "Delete";
        //    }
        //    if (actionName.Contains("UPDATE"))
        //    {
        //        actionType = "Update";
        //    }
        //    if (actionName.Contains("INSERT"))
        //    {
        //        actionType = "Insert";
        //    }
        //    _hubContext.Clients.All.SendAsync("ReceiveMessage", this.GetAlarmList());
        //}
        //public string GetAlarmList()
        //{
        //    DataTable dataTable = new DataTable();
        //    using (NpgsqlCommand sqlCmd = new NpgsqlCommand())
        //    {
        //        sqlCmd.CommandType = CommandType.StoredProcedure;
        //        sqlCmd.CommandText = "sp_alarm_speed_process_get";
        //        NpgsqlConnection conn = new NpgsqlConnection(_appSettings.ConnectionString);
        //        conn.Open();
        //        sqlCmd.Connection = conn;
        //        using (NpgsqlDataReader reader = sqlCmd.ExecuteReader())
        //        {
        //            dataTable.Load(reader);
        //            reader.Close();
        //            conn.Close();
        //        }
        //    }

        //    string json = JsonConvert.SerializeObject(dataTable);

        //    _cache.Set("SpeedAlarm", json);
        //    return _cache.Get("SpeedAlarm").ToString();
        //}
        //private void conn_StateChange(object sender, StateChangeEventArgs e)
        //{
        //    _hubContext.Clients.All.SendAsync("Current State: " + e.CurrentState.ToString() + " Original State: " + e.OriginalState.ToString(), "connection state changed");
        //}
    }
}
