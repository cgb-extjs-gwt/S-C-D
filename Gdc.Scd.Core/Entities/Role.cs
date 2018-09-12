using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Entities
{
    public class Role : NamedId
    {
        public bool IsGlobal { get; set; }
    }
}
