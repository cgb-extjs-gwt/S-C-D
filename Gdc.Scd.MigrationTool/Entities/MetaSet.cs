using Gdc.Scd.Core.Meta.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.MigrationTool.Entities
{
    public class MetaSet
    {
        public DomainMeta DomainMeta { get; set; }

        public DomainEnitiesMeta EnitiesMeta { get; set; }
    }
}
