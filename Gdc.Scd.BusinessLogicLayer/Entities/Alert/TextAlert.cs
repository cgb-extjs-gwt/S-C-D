using System;

namespace Gdc.Scd.BusinessLogicLayer.Entities.Alert
{
    public class TextAlert
    {
        public const string ALERT_DEFAULT = "DEFAULT";

        public const string ALERT_ERROR = "ERROR";

        public const string ALERT_INFO = "INFO";

        public const string ALERT_LINK = "LINK";

        public const string ALERT_REPORT = "REPORT";

        public const string ALERT_SUCCESS = "SUCCESS";

        public const string ALERT_WARNING = "WARNING";

        public string Type { get; }

        public string Caption { get; }

        public string Text { get; }

        protected TextAlert(
                string type,
                string caption,
                string text
            )
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("Invalid type");
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Invalid text");
            }

            this.Type = type;
            this.Caption = caption;
            this.Text = text;
        }

        public static TextAlert Default(string text)
        {
            return new TextAlert(ALERT_DEFAULT, "Info", text);
        }

        public static TextAlert Error(string text)
        {
            return new TextAlert(ALERT_ERROR, "Error", text);
        }

        public static TextAlert Info(string text)
        {
            return new TextAlert(ALERT_INFO, "Info", text);
        }

        public static TextAlert Success(string text)
        {
            return new TextAlert(ALERT_SUCCESS, "Success", text);
        }

        public static TextAlert Warning(string text)
        {
            return new TextAlert(ALERT_WARNING, "Warning", text);
        }

        public static LinkAlert Link(string text, string url)
        {
            return new LinkAlert(url, "Success", text);
        }

        public static ReportAlert Report(string text, string url)
        {
            return new ReportAlert(url, "Success", text);
        }
    }
}
