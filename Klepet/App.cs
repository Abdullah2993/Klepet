using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using Antlr.Runtime.Misc;
using log4net;
using StackExchange.Redis;

namespace Klepet
{
    public static class App
    {
        public static ConnectionMultiplexer Redis { get; } =
            ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = {ConfigurationManager.AppSettings["redisServer"]},
                AbortOnConnectFail = bool.Parse(ConfigurationManager.AppSettings["redisAbort"])
            });

        public static string InstanceId { get; } = ConfigurationManager.AppSettings["instanceId"]; //TODO validate
        public static string ConnectionChannel { get; } = ConfigurationManager.AppSettings["connectionChannel"];
        public static string ChatChannel { get; } = ConfigurationManager.AppSettings["chatChannel"];
        public static string ConnectionList { get; } = ConfigurationManager.AppSettings["connectionList"];

        public static ConcurrentDictionary<string,byte> ClientList { get; } = new ConcurrentDictionary<string, byte>(); //TODO #IYKWIM #LOL
    }
}