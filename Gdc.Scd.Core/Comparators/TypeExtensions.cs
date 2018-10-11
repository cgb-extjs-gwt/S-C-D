using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Comparators
{
    public static class TypeExtensions
    {
        public static bool IsSimpleType(
         this Type type)
        {
            return
                   type.IsValueType ||
                   type.IsPrimitive ||
                   type == typeof(String) ||
                   (Convert.GetTypeCode(type) != TypeCode.Object);
        }

        public static bool IsNumericType(this Type type)
        {
            return type == typeof(Int16) || type == typeof(Int32) ||
                type == typeof(Int64) || type == typeof(Decimal) ||
                type == typeof(Double) || type == typeof(Single) ||
                type == typeof(UInt16) || type == typeof(UInt32) ||
                type == typeof(UInt64) || type == typeof(Nullable<Int16>) || type == typeof(Nullable<Int32>)
                || type == typeof(Nullable<Int64>) || type == typeof(Nullable<Decimal>) ||
                type == typeof(Nullable<Double>) || type == typeof(Nullable<Single>) ||
                type == typeof(Nullable<UInt16>) || type == typeof(Nullable<UInt32>) ||
                type == typeof(Nullable<UInt64>);
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            if (type == typeof(string))
                return String.Empty;

            return null;
        }
    }
}
