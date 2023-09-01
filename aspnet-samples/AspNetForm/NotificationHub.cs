using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AspNetForm
{
    public class NotificationHub : Hub
    {
        public async Task<List<NotificationItem>> ListenGroup(string group)
        {
            await Groups.Add(Context.ConnectionId, group);
            return Database.Instance.GetItemsByGroup(group);
        }

        public Task<NotificationItem> Append(string group, string message)
        {
            var item = Database.Instance.Add(group, message);
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                Database.Instance.SetProcessed(item.Id);
                GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().Clients.Group(item.Group).UpdateItem(item);
            });
            return Task.FromResult(item);
        }
    }
}