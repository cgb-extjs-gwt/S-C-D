namespace Gdc.Scd.BusinessLogicLayer.Entities.Alert
{
    public class ReportAlert : TextAlert
    {
        public string Url { get; }

        public ReportAlert(string url, string caption, string text) : base(ALERT_REPORT, caption, text)
        {
            this.Url = url;
        }
    }
}
