using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Extensions
{
    public static class EntityExtensions
    {
        public static void CopyModifiedValues<T>(this T sourceEntity, T targetEntity, DateTime modified) where T : IDeactivatable
        {
            bool isModified = false;
            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                foreach (Attribute attr in pi.GetCustomAttributes())
                {
                    if (attr is MustCompareAttribute)
                    {
                        var prop1Value = pi.GetValue(sourceEntity, null);
                        pi.SetValue(targetEntity, prop1Value, null);
                        isModified = true;
                    }
                }
            }

            targetEntity.DeactivatedDateTime = null;
            if (targetEntity.CreatedDateTime == DateTime.MinValue)
                targetEntity.CreatedDateTime = modified;

            if (isModified)
            {
                targetEntity.ModifiedDateTime = modified;
            }
        }
    }
}
