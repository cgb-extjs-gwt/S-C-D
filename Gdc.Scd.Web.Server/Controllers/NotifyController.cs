using Gdc.Scd.BusinessLogicLayer.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Gdc.Scd.Web.Server.Controllers
{
    public class NotifyController : ApiController
    {
        private static readonly string HELLO_MSG = new { type = "<HELLO>" }.AsJson();

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

            Send(context.Response, HELLO_MSG);

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

                context.Response.End();
            });

            t.Start();
            return t;
        }

        private static void Send(HttpResponse resp, object value)
        {
            Send(resp, value.AsJson());
        }

        private static void Send(HttpResponse resp, string value)
        {
            resp.Write(value);
            resp.Write("\n---\n");
            resp.Flush();
        }
    }
}