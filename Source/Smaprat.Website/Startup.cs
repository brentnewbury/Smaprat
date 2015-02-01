using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System;
using System.Configuration;

[assembly: OwinStartup(typeof(Smaprat.Website.Startup))]
namespace Smaprat.Website
{
    // Owin startup class
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureServiceBusBackplane();

            app.MapSignalR();
        }

        private void ConfigureServiceBusBackplane()
        {
            string connectionString = ConfigurationManager.AppSettings["site:ServiceBusConnectionString"];
            if (String.IsNullOrWhiteSpace(connectionString))
                return;

            GlobalHost.DependencyResolver.UseServiceBus(connectionString, "Smaprat");
        }
    }
}