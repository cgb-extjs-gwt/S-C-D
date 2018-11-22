namespace Gdc.Scd.BusinessLogicLayer.Entities.Alert
{
    public class LinkAlert : TextAlert
    {
        public string Url { get; }

        public LinkAlert(string url, string caption, string text) : base(ALERT_LINK, caption, text)
        {
            this.Url = url;
        }
    }
}
