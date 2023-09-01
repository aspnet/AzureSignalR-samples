using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;
using System.Web.UI;

namespace AspNetForm
{
    public partial class _Default : Page
    {
        private const string GroupName = "default";

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            var item = Database.Instance.Add(GroupName, txtMessage.Text);
            GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().Clients.Group(GroupName).UpdateItem(item);

            Task.Run(async () =>
            {
                await Task.Delay(10000);
                Database.Instance.SetProcessed(item.Id);
                GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().Clients.Group(item.Group).UpdateItem(item);
            });
        }
    }
}