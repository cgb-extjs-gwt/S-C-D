using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MustCompareAttribute : System.Attribute
    {
        public bool IsComparable { get; set; }
        public bool IsIgnoreCase { get; set; }

        public MustCompareAttribute()
        {
            IsComparable = true;
        }

        public MustCompareAttribute(bool isComparable)
        {
            IsComparable = isComparable;
        }
    }
}
