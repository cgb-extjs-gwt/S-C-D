using Gdc.Scd.Core.Interfaces;
using System;

namespace Gdc.Scd.Core.Entities
{
    public class CostElementIdentifier : ICostElementIdentifier, ICloneable
    {
        public string CostElementId { get; set; }

        public string ApplicationId { get; set; }

        public string CostBlockId { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
