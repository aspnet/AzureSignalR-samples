using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(AspNetForm.Startup))]

namespace AspNetForm
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // app.MapSignalR();
            app.UseCors(CorsOptions.AllowAll);
            app.MapAzureSignalR(GetType().FullName);
        }
    }
}