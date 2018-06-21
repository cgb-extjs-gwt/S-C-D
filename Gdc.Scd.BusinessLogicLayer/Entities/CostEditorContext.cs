using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class CostEditorContext
    {
        public string ApplicationId { get; set; }

        public string ScopeId { get; set; }

        public string CountryId { get; set; }

        public string CostBlockId { get; set; }

        public string CostElementId { get; set; }

        public string InputLevelId { get; set; }

        public string[] CostElementFilterIds { get; set; }

        public string[] InputLevelFilterIds { get; set; }
    }
}
