using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using StackExchange.Redis;
using static Klepet.App;

namespace Klepet.Hubs
{
    [HubName("chatHub")]
    public class ChatHub:Hub
    {
        static ChatHub()
        {
            if (Redis.IsConnected)
            {
                Redis.GetSubscriber().SubscribeAsync(ConnectionChannel, GetConnectionUpdates);
                Redis.GetSubscriber().SubscribeAsync(ChatChannel, GetMessageUpdates);
            }
        }

        [HubMethodName("sendMessage")]
        public void SendMessage(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
                return;

            if (Redis.IsConnected)
                    Redis.GetSubscriber().PublishAsync(ChatChannel, $"{InstanceId}|{Context.ConnectionId}|{message}");

            Clients.AllExcept(Context.ConnectionId).receiveMessage(Context.ConnectionId, message);
        }

        public override Task OnConnected()
        {
            if (Redis.IsConnected)
            {
                Redis.GetDatabase().SetAddAsync(ConnectionList, Context.ConnectionId);
                Redis.GetSubscriber().PublishAsync("connections", $"1|{InstanceId}|{Context.ConnectionId}");
            }
            else
            {
                if (!ClientList.ContainsKey(Context.ConnectionId))
                        ClientList.TryAdd(Context.ConnectionId, 0);
            }

            Clients.All.clientConnected(Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (Redis.IsConnected)
            {
                Redis.GetDatabase().SetRemoveAsync(ConnectionList, Context.ConnectionId);
                Redis.GetSubscriber().PublishAsync("connections", $"0|{InstanceId}|{Context.ConnectionId}");
            }
            else
            {
                if (ClientList.ContainsKey(Context.ConnectionId))
                {
                    byte temp;
                    ClientList.TryRemove(Context.ConnectionId, out temp);
                }
            }

            GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.clientDisconnected(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        private static void GetMessageUpdates(RedisChannel channel, RedisValue value)
        {
            var vals = value.ToString().Split('|');
            if (vals.Length != 3 || vals[0] == InstanceId) return;

            GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.receiveMessage(vals[1], vals[2]);
        }

        private static void GetConnectionUpdates(RedisChannel channel, RedisValue value)
        {
            var vals = value.ToString().Split('|');
            if (vals.Length != 3 || vals[1] == InstanceId) return;
            switch (vals[0])
            {
                case "1":
                    GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.clientConnected(vals[2]);
                    break;
                case "0":
                    GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.clientDisconnected(vals[2]);
                    break;
            }
        }
    }
}