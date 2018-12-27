using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Attributes
{
    [AttributeUsage(System.AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MustUpdateCoordinateAttribute : Attribute
    {
        public string CoordinateName { get; set; }

        public MustUpdateCoordinateAttribute()
        {

        }

        public MustUpdateCoordinateAttribute(string coordinateName)
        {
            CoordinateName = coordinateName;
        }
    }
}
