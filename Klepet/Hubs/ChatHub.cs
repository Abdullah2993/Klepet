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
                Redis.GetSubscriber().Subscribe(ConnectionChannel, GetConnectionUpdates);
                Redis.GetSubscriber().Subscribe(ChatChannel, GetMessageUpdates);
            }
        }

        [HubMethodName("sendMessage")]
        public async Task SendMessage(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
                return;

            if (Redis.IsConnected)
                    await Redis.GetSubscriber().PublishAsync(ChatChannel, $"{InstanceId}|{Context.ConnectionId}|{message}");

            Clients.AllExcept(Context.ConnectionId).receiveMessage(Context.ConnectionId, message);
        }

        public override async Task OnConnected()
        {
            if (Redis.IsConnected)
            {
                await Redis.GetDatabase().SetAddAsync(ConnectionList, Context.ConnectionId);
                await Redis.GetSubscriber().PublishAsync(ConnectionChannel, $"1|{InstanceId}|{Context.ConnectionId}");
            }
            else
            {
                if (!ClientList.ContainsKey(Context.ConnectionId))
                        ClientList.TryAdd(Context.ConnectionId, 0);
            }

            Clients.All.clientConnected(Context.ConnectionId);

            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            if (Redis.IsConnected)
            {
                await Redis.GetDatabase().SetRemoveAsync(ConnectionList, Context.ConnectionId);
                await Redis.GetSubscriber().PublishAsync(ConnectionChannel, $"0|{InstanceId}|{Context.ConnectionId}");
            }
            else
            {
                if (ClientList.ContainsKey(Context.ConnectionId))
                {
                    byte temp;
                    ClientList.TryRemove(Context.ConnectionId, out temp);
                }
            }

            Clients.Others.clientDisconnected(Context.ConnectionId);

            await base.OnDisconnected(stopCalled);
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

            if (vals[0] == "1")
            {
                GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.clientConnected(vals[2]);
            }
            else if (vals[0] == "0")
            {
                GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.All.clientDisconnected(vals[2]);
            }
        }
    }
}