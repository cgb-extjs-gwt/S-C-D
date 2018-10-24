using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    public class Enums
    {
        public static class InputFileCoumns
        {
            public static readonly int FspCode = 1;
            public static readonly int ServiceLocation = 3;
            public static readonly int Availability = 4;
            public static readonly int ReactionTime = 5;
            public static readonly int ReactionType = 6;
            public static readonly int WarrantyGroup = 7;
            public static readonly int Duration = 8;
        }

        public static class CdCsFileCoumns
        {
            public static readonly int ServiceTC = 9;
            public static readonly int ServiceTP = 10;
        }
    }
}
