using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.Scripts;
using Gdc.Scd.Tests.Integration.Import.Por.Helpers;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class UpdateSwCostTest : UpdateSwCost
    {
        public const string RESULT_PATH = "Results";

        public UpdateSwCostTest() : base(new NamedId[1]) { }

        [TestCase]
        public void SqlBySogTest()
        {
            this.items = InputAtomHelper.CreateWg("aa1", "xyz", "abc");
            this.table = "SoftwareSolution.SwSpMaintenance";
            this.deps = new string[] { "Sog", "DurationAvailability" };
            this.updateFields = GetFields();

            var sql = TransformText();

            sql.Has("('AA1', 'XYZ', 'ABC')");
            sql.Has("Sog", "Sog not found");
            sql.Has("create index ix_tmp_Country_SLA on #tmp([Sog], [DurationAvailability]);", "index ix_tmp_Country_SLA");
            sql.Has("create index ix_tmpmin_Country_SLA on #tmpMin([Sog], [DurationAvailability]);", "ix_tmpmin_Country_SLA");

            StreamUtil.Save(RESULT_PATH, "update_sw_maintenance_by_sog.sql", sql);
        }

        private static string[] GetFields()
        {
            return new string[] {
              "2ndLevelSupportCosts",
              "ReinsuranceFlatfee",
              "CurrencyReinsurance",
              "RecommendedSwSpMaintenanceListPrice",
              "MarkupForProductMarginSwLicenseListPrice",
              "ShareSwSpMaintenanceListPrice",
              "DiscountDealerPrice"
            };
        }
    }
}
