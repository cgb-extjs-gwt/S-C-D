using Gdc.Scd.Import.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ParseInfoAttribute : Attribute
    {
        public int ColumnNumber { get; set; }
        public Format Format { get; set; }

        public ParseInfoAttribute(int columnNumber)
        {
            ColumnNumber = columnNumber;
        }
    }
}
