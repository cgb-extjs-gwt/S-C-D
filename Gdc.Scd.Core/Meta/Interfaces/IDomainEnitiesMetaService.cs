using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Interfaces
{
    public interface IDomainEnitiesMetaService
    {
        DomainEnitiesMeta Get(DomainMeta domainMeta);
    }
}
