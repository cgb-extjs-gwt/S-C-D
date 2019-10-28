using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gdc.Scd.Import.Por.Core.Extensions
{
    public static class EntityExtensions
    {
        public static void CopyModifiedValues<T>(this T sourceEntity, T targetEntity, DateTime modified) 
            where T : IModifiable
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

        public static UpdateQueryOption GetUpdatedCoordinates<T>(this T newEntity, T oldEntity)
            where T: IIdentifiable
        {
            var classAttribute = Attribute.GetCustomAttribute(typeof(T), typeof(MustUpdateCoordinateAttribute));

            if (classAttribute != null)
            {
                var updateCoordinateAttribute = (MustUpdateCoordinateAttribute)classAttribute;

                var oldCoordinates = new Dictionary<string, long>();
                var newCoordinates = new Dictionary<string, long>();

                foreach (PropertyInfo pi in typeof(T).GetProperties())
                {
                    foreach (Attribute attr in pi.GetCustomAttributes())
                    {
                        if (attr is MustUpdateCoordinateAttribute)
                        {
                            var mustUpdateAttr = (MustUpdateCoordinateAttribute)attr;
                            var prop1Value = pi.GetValue(oldEntity, null);
                            var prop2Value = pi.GetValue(newEntity, null);

                            try
                            {
                                var longOldVal = Convert.ToInt64(prop1Value);
                                var longNewVal = Convert.ToInt64(prop2Value);

                                if (longOldVal != longNewVal)
                                {
  
                                    oldCoordinates.Add(mustUpdateAttr.CoordinateName, longOldVal);
                                    newCoordinates.Add(mustUpdateAttr.CoordinateName, longNewVal);
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                        }
                    }
                }

                if (oldCoordinates.Any() && newCoordinates.Any())
                {
                    oldCoordinates.Add(updateCoordinateAttribute.CoordinateName, oldEntity.Id);
                    newCoordinates.Add(updateCoordinateAttribute.CoordinateName, oldEntity.Id);

                    return new UpdateQueryOption(oldCoordinates, newCoordinates);
                }
            }

            return null;
        }
    }
}
