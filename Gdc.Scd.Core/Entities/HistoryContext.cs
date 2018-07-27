using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class HistoryContext
    {
        //public long Id { get; set; }

        public string ApplicationId { get; set; }

        public string RegionInputId { get; set; }

        public string CostBlockId { get; set; }

        public string CostElementId { get; set; }

        public string InputLevelId { get; set; }

        //public List<SimpleValue<long>> CostElementFilterIds { get; set; }

        //public List<SimpleValue<long>> InputLevelFilterIds { get; set; }
    }
}
