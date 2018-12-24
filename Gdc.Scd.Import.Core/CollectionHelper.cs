using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core
{
    public static class CollectionHelper
    {
        public static void AddEntry<T>(Dictionary<string, T> collection, T item, ILogger<LogLevel> logger) 
            where T : NamedId
        {
            if (!collection.ContainsKey(item.Name))
            {
                logger.Log(LogLevel.Debug, ImportConstants.UPDATING_ENTITY, typeof(T).Name, item.Name);
                collection.Add(item.Name, item);
            }
        }
    }
}
