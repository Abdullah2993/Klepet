using System;
using Microsoft.AspNet.SignalR;
using Owin;

namespace Klepet
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(10);
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(2);
        }
    }
}