using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Smaprat.Website.Startup))]
namespace Smaprat.Website
{
    // Owin startup class
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}