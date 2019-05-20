using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Dto
{
    public struct ArchiveFolderDto
    {
        public string Name { get; set; }
        public int FileCount { get; set; }
        public double TotalFolderSize { get; set; }
    }
}
