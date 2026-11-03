using System.Threading.Tasks;
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
            var corsPolicy = new CorsPolicy
            {
                AllowAnyMethod = true,
                AllowAnyHeader = true
            };
            corsPolicy.Origins.Add("https://localhost:44300");
            var corsOptions = new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context => Task.FromResult<CorsPolicy>(corsPolicy)
                }
            };
            app.UseCors(corsOptions);
            app.MapAzureSignalR(GetType().FullName);
        }
    }
}