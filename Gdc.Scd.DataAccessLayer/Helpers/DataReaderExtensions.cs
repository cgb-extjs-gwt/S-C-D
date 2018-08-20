using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class DataReaderExtensions
    {
        public static List<T> MapToList<T>(this DbDataReader dataReader) where T : new()
        {
            if (dataReader != null && dataReader.HasRows)
            {
                var entity = typeof(T);
                var entities = new List<T>();
                var propDictionary = new Dictionary<string, PropertyInfo>();
                var props = entity.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                propDictionary = props.ToDictionary(p => p.Name.ToUpper(), p => p);

                while (dataReader.Read())
                {
                    T newObj = new T();
                    for (int index = 0; index < dataReader.FieldCount; index++)
                    {
                        if (propDictionary.ContainsKey(dataReader.GetName(index).ToUpper()))
                        {
                            var info = propDictionary[dataReader.GetName(index).ToUpper()];
                            if (info != null && info.CanWrite)
                            {
                                var value = dataReader.GetValue(index);
                                info.SetValue(newObj, (value == DBNull.Value) ? default(T) : value, null);
                            }
                        }
                    }

                    entities.Add(newObj);
                }

                return entities;
            }

            return null;
        }
    }
}
