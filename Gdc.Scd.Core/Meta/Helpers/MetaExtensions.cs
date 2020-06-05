using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Meta.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Helpers
{
    public static class MetaExtensions
    {
        public static IEnumerable<string> ToNames(this IEnumerable<FieldMeta> fields)
        {
            return fields.Select(field => field.Name);
        }

        public static string[] ToNamesArray(this IEnumerable<FieldMeta> fields)
        {
            return fields.ToNames().ToArray();
        }

        public static IEnumerable<T> WithoutIdField<T>(this IEnumerable<T> fields)
            where T : FieldMeta
        {
            return fields.Where(field => !(field is IdFieldMeta));
        }

        public static IEnumerable<FieldMeta> WithoutTypeField<T>(this IEnumerable<FieldMeta> fields)
            where T : FieldMeta
        {
            return fields.Where(field => !(field is T));
        }

        public static IEnumerable<T> WithoutField<T>(this IEnumerable<T> fields, FieldMeta field)
            where T : FieldMeta
        {
            return fields.Where(x => x != field);
        }

        public static IEnumerable<FieldMeta> ConcatFields(this IEnumerable<FieldMeta> fields, FieldMeta field)
        {
            return fields.Concat(field);
        }

        public static IEnumerable<FieldMeta> ConcatFields(this IEnumerable<FieldMeta> fields, FieldMeta field1, FieldMeta field2)
        {
            return fields.Concat(field1, field2);
        }

        public static IEnumerable<FieldMeta> ConcatFields(
            this IEnumerable<FieldMeta> fields, 
            FieldMeta field1, 
            FieldMeta field2, 
            FieldMeta field3)
        {
            return fields.Concat(field1, field2, field3);
        }

        public static IEnumerable<FieldMeta> ConcatFields(
            this IEnumerable<FieldMeta> fields,
            FieldMeta field1,
            FieldMeta field2,
            FieldMeta field3,
            FieldMeta field4)
        {
            return fields.Concat(field1, field2, field3, field4);
        }
    }
}
