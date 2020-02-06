using Gdc.Scd.Core.Interfaces;
using System;

namespace Gdc.Scd.Web.Server.Entities
{
    public class RequestInfo : IIdentifiable
    {
        public long Id { get; set; }

        public DateTime DateTime { get; set; }

        public long Duration { get; set; }

        public string UserLogin { get; set; }

        public string RequestType { get; set; }

        public string Host { get; set; }

        public string Url { get; set; }

        public string Error { get; set; }
    }
}