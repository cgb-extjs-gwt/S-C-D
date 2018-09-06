using System;

namespace Gdc.Scd.Core.Dto
{
    public class HistoryItem
    {
        public DateTime EditDate { get; set; }

        public long EditUserId { get; set; }

        public string EditUserName { get; set; }

        public object Value { get; set; }
    }
}
