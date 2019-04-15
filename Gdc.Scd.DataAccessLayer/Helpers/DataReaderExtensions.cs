using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gdc.Scd.DataAccessLayer.Helpers
{
    public static class DataReaderExtensions
    {
        public static List<T> MapToList<T>(this DbDataReader reader) where T : new()
        {
            if (IsEmpty(reader))
            {
                return null;
            }

            var entity = typeof(T);
            var entities = new List<T>();
            var propDictionary = new Dictionary<string, PropertyInfo>();
            var props = entity.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            propDictionary = props.ToDictionary(p => p.Name.ToUpper(), p => p);

            while (reader.Read())
            {
                T newObj = new T();
                for (int index = 0; index < reader.FieldCount; index++)
                {
                    if (propDictionary.ContainsKey(reader.GetName(index).ToUpper()))
                    {
                        var info = propDictionary[reader.GetName(index).ToUpper()];
                        if (info != null && info.CanWrite)
                        {
                            var value = reader.GetValue(index);
                            info.SetValue(newObj, (value == DBNull.Value) ? default(T) : value, null);
                        }
                    }
                }

                entities.Add(newObj);
            }

            return entities;
        }

        public static (string json, int total) MapToJsonArray(this DbDataReader reader)
        {
            if (IsEmpty(reader))
            {
                return ("[]", 0);
            }

            int total = 0;
            var sb = new StringBuilder(512);

            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                total = WriteJsonArray(reader, writer);
            }

            return (sb.ToString(), total);
        }

        public static Stream MapToJsonArrayStream(this DbDataReader reader)
        {
            if (IsEmpty(reader))
            {
                return null;
            }

            var ms = new MemoryStream(2048);

            using (var streamWriter = new StreamWriter(ms))
            {
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    WriteJsonArray(reader, writer);
                }
            }

            ms.Position = 0;

            return ms;
        }

        public static DataTable MapToTable(this DbDataReader reader)
        {
            if (IsEmpty(reader))
            {
                return null;
            }

            var tbl = new DataTable();
            tbl.Load(reader);
            return tbl;
        }

        public static bool HasField(this DbDataReader reader, string field)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.CompareOrdinal(reader.GetName(i), field) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsEmpty(DbDataReader reader)
        {
            return reader == null || !reader.HasRows || reader.FieldCount <= 0;
        }

        private static int WriteJsonArray(DbDataReader reader, JsonWriter writer)
        {
            int total = 0;
            int i, fieldCount = reader.FieldCount;

            writer.WriteStartArray();

            while (reader.Read())
            {
                total++;
                writer.WriteStartObject();

                for (i = 0; i < fieldCount; i++)
                {
                    if (reader.IsDBNull(i))
                    {
                        continue;
                    }

                    writer.WritePropertyName(reader.GetName(i));
                    writer.WriteValue(reader.GetValue(i));
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            return total;
        }
    }
}
