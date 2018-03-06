using System.Configuration;
using Owin;

namespace ChatRoomAspNet
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseAzureSignalR(ConfigurationManager.AppSettings["AzureSignalRConnectionString"],
                builder =>
                {
                    builder.UseHub<Chat>();
                });
        }
    }
}