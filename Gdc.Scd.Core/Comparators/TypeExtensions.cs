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
    }
}
