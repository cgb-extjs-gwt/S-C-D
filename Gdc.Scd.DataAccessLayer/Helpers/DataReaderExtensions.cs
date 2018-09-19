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

        public static string MapAsJson(this DbDataReader reader)
        {
            if (reader == null ||
                !reader.HasRows ||
                reader.FieldCount <= 0)
            {
                return null;
            }

            var sb = new StringBuilder(512);
            int i, fieldCount = reader.FieldCount;
            bool flag = false;

            sb.Append('[');

            while (reader.Read())
            {
                if (flag)
                {
                    sb.Append(',');
                }

                sb.Append('{');

                for (i = 0; i < fieldCount; i++)
                {
                    if (propDictionary.ContainsKey(reader.GetName(i).ToUpper()))
                    {
                        var info = propDictionary[reader.GetName(i).ToUpper()];
                        if (info != null && info.CanWrite)
                        {
                            var value = reader.GetValue(i);
                            info.SetValue(newObj, (value == DBNull.Value) ? default(T) : value, null);
                        }
                    }
                }

                sb.Append('}');
            }

            sb.Append(']');

            return sb.ToString();
        }
    }
}
