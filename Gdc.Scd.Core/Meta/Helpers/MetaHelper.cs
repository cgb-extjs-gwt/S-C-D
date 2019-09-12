using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Meta.Helpers
{
    public static class MetaHelper
    {
        public static EntityInfo GetEntityInfo(Type entityType)
        {
            var entityInfo = new EntityInfo();

            var tableAttr = entityType.GetCustomAttributes(false).OfType<TableAttribute>().FirstOrDefault();
            if (tableAttr == null)
            {
                entityInfo.Name = entityType.Name;
                entityInfo.Schema = MetaConstants.DefaultSchema;
            }
            else
            {
                entityInfo.Name = tableAttr.Name;
                entityInfo.Schema = tableAttr.Schema ?? MetaConstants.DefaultSchema;
            }

            return entityInfo;
        }

        public static EntityInfo GetEntityInfo<T>()
        {
            return GetEntityInfo(typeof(T));
        }
    }
}
