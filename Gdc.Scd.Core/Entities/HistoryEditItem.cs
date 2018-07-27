using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class HistoryEditItem : IIdentifiable
    {
        public long Id { get; set; }

        public double Value { get; set; }
    }
}
