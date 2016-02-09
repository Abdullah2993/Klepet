using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Klepet.Models;
using StackExchange.Redis;
using static Klepet.App;

namespace Klepet.Controllers
{
    public class HomeController : Controller
    {
        public ViewResult Index()
        {           
            return View();
        }

        /*public async Task<PartialViewResult> Clients()
        {
            var clients = new Clients();
            if (Redis.IsConnected)
            {
                var ids = await Redis.GetDatabase().SetMembersAsync(ConnectionList);
                clients.Ids = ids.ToStringArray();
            }
            else
            {
                clients.Ids = ClientList.Keys.ToArray();
            }
            return PartialView(clients);
        } */

        public PartialViewResult Clients()
        {
            var clients = new Clients();
            if (Redis.IsConnected)
            {
                var ids =  Redis.GetDatabase().SetMembers(ConnectionList);
                clients.Ids = ids.ToStringArray();
            }
            else
            {
                clients.Ids = ClientList.Keys.ToArray();
            }
            return PartialView(clients);
        }
    }
}