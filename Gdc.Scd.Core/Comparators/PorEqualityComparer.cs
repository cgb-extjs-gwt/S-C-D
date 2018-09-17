using Gdc.Scd.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Comparators
{
    public class PorEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            bool result = true;

            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                foreach (Attribute attr in pi.GetCustomAttributes())
                {
                    if (attr is MustCompareAttribute)
                    {
                        MustCompareAttribute compare = (MustCompareAttribute)attr;
                        if (compare.IsComparable)
                        {
                            var prop1Value = pi.GetValue(x, null);
                            var prop2Value = pi.GetValue(y, null);

                            if (prop1Value == null && prop2Value == null)
                            {
                                result = result && true;
                            }
                            else if (prop1Value == null | prop2Value == null)
                            {
                                result = result && false;
                            }
                            else
                            {
                                if (pi.PropertyType == typeof(string))
                                {
                                    result = result &&
                                        compare.IsIgnoreCase ? String.Equals((string)prop1Value,
                                        (string)prop2Value, StringComparison.OrdinalIgnoreCase) : String.Equals((string)prop1Value,
                                        (string)prop2Value);
                                }
                                else
                                {
                                    result = result && prop1Value.Equals(prop2Value);
                                }
                            }

                            if (!result)
                                return false;
                        }
                    }
                }
            }

            return result;
        }

        public int GetHashCode(T obj)
        {
            int result = 0;

            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                if (pi.PropertyType.IsSimpleType())
                {
                    var prop1Value = pi.GetValue(obj, null);
                    if (prop1Value != null)
                    {
                        result = result ^ prop1Value.GetHashCode();
                    }
                } 
            }

            return result;
        }
    }
}
