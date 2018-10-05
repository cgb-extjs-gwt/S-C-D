using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class ParseInfoDto
    {
        public string Delimeter { get; set; }
        public StreamReader Content { get; set; }
        public bool HasHeader { get; set; }
    }
}
