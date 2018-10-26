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

            SendHello(context.Response);

            Task t = new Task(() =>
            {
                //2 minutes waiting...

                for (int n = 0; n < 240; n++)
                {
                    var msg = channel.GetMessage(username);
                    if (msg != null)
                    {
                        Send(context.Response, msg);
                        channel.RemoveMessage(username, msg);
                    }

                    Thread.Sleep(500);
                }
            });

            t.Start();
            return t;
        }

        private static void SendHello(HttpResponse resp)
        {
            Send(resp, new { type = "<HELLO>" });
        }

        private static void Send(HttpResponse resp, object value)
        {
            resp.Write(value.AsJson());
            resp.Write("---");
            resp.Flush();
        }
    }
}