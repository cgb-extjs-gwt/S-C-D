using System;
using System.Collections.Generic;

namespace Gdc.Scd.Core.Interfaces
{
    public interface IRegisteredEntitiesProvider
    {
        IEnumerable<Type> GetRegisteredEntities();
    }
}
