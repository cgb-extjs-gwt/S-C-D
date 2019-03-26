using ClosedXML.Excel;
using Gdc.Scd.Export.CdCs.Impl;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.OperationResult;

namespace Gdc.Scd.Export.CdCs
{
    class Program
    {
        static void Main(string[] args)
        {
            CdCsService.DoThings();
        }

        public OperationResult<bool> Output()
        {
            return CdCsService.DoThings();
        }

        public string WhoAmI()
        {
            return "CdCsJob";
        }
    }
}
