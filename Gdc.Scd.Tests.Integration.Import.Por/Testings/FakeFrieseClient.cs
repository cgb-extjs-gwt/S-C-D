using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Por;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Tests.Util;
using System.Collections.Generic;

namespace Gdc.Scd.Tests.Integration.Import.Por.Testings
{
    public class FakeFrieseClient : FrieseClient
    {
        public FakeFrieseClient(ILogger log) : base(log) { }

        public override List<SCD2_WarrantyGroups> GetWg()
        {
            return Json<SCD2_WarrantyGroups>("wg.json");
        }

        public override List<SCD2_ServiceOfferingGroups> GetSog()
        {
            return Json<SCD2_ServiceOfferingGroups>("sog.json.txt");
        }

        public override List<SCD2_SW_Overview> GetSw()
        {
            return Json<SCD2_SW_Overview>("sw.json.txt");
        }


        protected override List<SCD2_v_SAR_new_codes> LoadFsp()
        {
            return Json<SCD2_v_SAR_new_codes>("fsp.json.txt");
        }

        public override List<SCD2_LUT_TSP> GetLut()
        {
            return Json<SCD2_LUT_TSP>("lut.json.txt");
        }

        public override List<SCD2_SWR_Level> GetSwProactive()
        {
            return Json<SCD2_SWR_Level>("sw-pro.json");
        }

        private static List<T> Json<T>(string fn)
        {
            return StreamUtil.ReadJson<List<T>>("TestData", fn);
        }
    }
}
