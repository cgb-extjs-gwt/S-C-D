using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class NotifyController : ApiController
    {
        INotifyChannel channel;

        public NotifyController(INotifyChannel channel)
        {
            this.channel = channel;
        }

        [HttpGet]
        public Task Connect()
        {
            return Connect(HttpContext.Current);
        }

        private Task Connect(HttpContext context)
        {
            context.ThreadAbortOnTimeout = false;
            //
            var username = context.Username();
            channel.Create(username);

            Task t = new Task(() =>
            {
                //2 minutes waiting...

                for (int n = 0; n < 240; n++)
                {
                    var msg = channel.GetMessage(username);
                    if (msg != null)
                    {
                        context.Response.Write(msg.AsJson());
                        context.Response.Flush();
                        channel.RemoveMessage(username, msg);
                    }

                    Thread.Sleep(500);
                }
            });

            t.Start();
            return t;
        }
    }
}