using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Interfaces
{
    public interface ICoordinateEntityMetaProvider
    {
        IEnumerable<NamedEntityMeta> GetCoordinateEntityMetas();
    }
}
