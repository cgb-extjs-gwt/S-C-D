﻿using System;

namespace Gdc.Scd.Core.Dto
{
    public class HistoryItemDto
    {
        public DateTime EditDate { get; set; }

        public long EditUserId { get; set; }

        public string EditUserName { get; set; }

        public object Value { get; set; }
    }
}
