using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.Scripts;
using Gdc.Scd.Tests.Integration.Import.Por.Helpers;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class UpdateCostTest : UpdateCost
    {
        public const string RESULT_PATH = "Results";

        public UpdateCostTest() : base(new NamedId[1]) { }

        [TestCase]
        public void SqlTest()
        {
            this.items = WgHelper.CreateWg("aa1", "xyz", "abc");
            this.table = "Hardware.LogisticsCosts";
            this.deps = new string[] { "Country", "Pla", "ReactionTimeType" };
            this.updateFields = GetFields();

            var sql = ByPla();

            sql.Has("('AA1', 'XYZ', 'ABC')");
            sql.Has("Pla", "Pla not found");
            sql.Has("create index ix_tmp_Country_SLA on #tmp([Country], [Pla], [ReactionTimeType]);", "index ix_tmp_Country_SLA");
            sql.Has("create index ix_tmpmin_Country_SLA on #tmpMin([Country], [Pla], [ReactionTimeType]);", "ix_tmpmin_Country_SLA");

            StreamUtil.Save(RESULT_PATH, "update_by_CentralContractGroup.sql", sql);
        }

        [TestCase("[Country], [Pla]", "Country", "Pla")]
        [TestCase("[Country], [Pla], [ReactionTimeType]", "Country", "Pla", "ReactionTimeType")]
        [TestCase("[Country], [Pla], [ReactionTimeTypeAvailability]", "Country", "Pla", "ReactionTimeTypeAvailability")]
        [TestCase("[Country], [CentralContractGroup], [ReactionTimeType]", "Country", "CentralContractGroup", "ReactionTimeType")]
        [TestCase("[Country], [CentralContractGroup]", "Country", "CentralContractGroup")]
        [TestCase("[Country], [CentralContractGroup], [ReactionTimeTypeAvailability]", "Country", "CentralContractGroup", "ReactionTimeTypeAvailability")]
        [TestCase("[Country], [CentralContractGroup]", "Country", "CentralContractGroup")]
        public void WriteDepsTest(string expected, params string[] deps)
        {
            this.deps = deps;
            GenerationEnvironment.Clear();
            WriteDeps();
            Assert.AreEqual(expected, GenerationEnvironment.ToString());
        }

        [TestCase("t.[Country] = c.[Country] and t.[CentralContractGroup] = c.[CentralContractGroup]", "Country", "CentralContractGroup")]
        [TestCase("t.[Country] = c.[Country] and t.[CentralContractGroup] = c.[CentralContractGroup] and t.[ReactionTimeTypeAvailability] = c.[ReactionTimeTypeAvailability]", "Country", "CentralContractGroup", "ReactionTimeTypeAvailability")]
        [TestCase("t.[Country] = c.[Country] and t.[Pla] = c.[Pla]", "Country", "Pla")]
        [TestCase("t.[Country] = c.[Country] and t.[Pla] = c.[Pla] and t.[ReactionTimeTypeAvailability] = c.[ReactionTimeTypeAvailability]", "Country", "Pla", "ReactionTimeTypeAvailability")]
        [TestCase("t.[Country] = c.[Country]", "Country")]
        public void WriteJoinByDepsTest(string expected, params string[] deps)
        {
            this.deps = deps;
            GenerationEnvironment.Clear();
            WriteJoinByDeps();
            Assert.AreEqual(expected, GenerationEnvironment.ToString());
        }

        [TestCase("StandardHandling", "case when min([StandardHandling]) = max([StandardHandling]) then min([StandardHandling]) else null end as [StandardHandling]")]
        [TestCase("HighAvailabilityHandling_Approved", "case when min([HighAvailabilityHandling_Approved]) = max([HighAvailabilityHandling_Approved]) then min([HighAvailabilityHandling_Approved]) else null end as [HighAvailabilityHandling_Approved]")]
        [TestCase("TaxiCourierDelivery", "case when min([TaxiCourierDelivery]) = max([TaxiCourierDelivery]) then min([TaxiCourierDelivery]) else null end as [TaxiCourierDelivery]")]
        public void WriteSelectFieldTest(string f, string expected)
        {
            GenerationEnvironment.Clear();
            base.WriteSelectField(f);
            Assert.AreEqual(expected, GenerationEnvironment.ToString());
        }

        [TestCase]
        public void WriteSelectFieldsTest()
        {
            this.updateFields = GetFields();
            GenerationEnvironment.Clear();
            base.WriteSelectFields();

            var sql = GenerationEnvironment.ToString();

            sql.Has("case when min([StandardHandling]) = max([StandardHandling]) then min([StandardHandling]) else null end as [StandardHandling]");
            sql.Has(", case when min([StandardHandling_Approved]) = max([StandardHandling_Approved]) then min([StandardHandling_Approved]) else null end as [StandardHandling_Approved]");
            sql.Has(", case when min([HighAvailabilityHandling]) = max([HighAvailabilityHandling]) then min([HighAvailabilityHandling]) else null end as [HighAvailabilityHandling]");
            sql.Has(", case when min([HighAvailabilityHandling_Approved]) = max([HighAvailabilityHandling_Approved]) then min([HighAvailabilityHandling_Approved]) else null end as [HighAvailabilityHandling_Approved]");
            sql.Has(", case when min([StandardDelivery]) = max([StandardDelivery]) then min([StandardDelivery]) else null end as [StandardDelivery]");
            sql.Has(", case when min([StandardDelivery_Approved]) = max([StandardDelivery_Approved]) then min([StandardDelivery_Approved]) else null end as [StandardDelivery_Approved]");
            sql.Has(", case when min([ExpressDelivery]) = max([ExpressDelivery]) then min([ExpressDelivery]) else null end as [ExpressDelivery]");
            sql.Has(", case when min([ExpressDelivery_Approved]) = max([ExpressDelivery_Approved]) then min([ExpressDelivery_Approved]) else null end as [ExpressDelivery_Approved]");
            sql.Has(", case when min([TaxiCourierDelivery]) = max([TaxiCourierDelivery]) then min([TaxiCourierDelivery]) else null end as [TaxiCourierDelivery]");
            sql.Has(", case when min([TaxiCourierDelivery_Approved]) = max([TaxiCourierDelivery_Approved]) then min([TaxiCourierDelivery_Approved]) else null end as [TaxiCourierDelivery_Approved]");
            sql.Has(", case when min([ReturnDeliveryFactory]) = max([ReturnDeliveryFactory]) then min([ReturnDeliveryFactory]) else null end as [ReturnDeliveryFactory]");
            sql.Has(", case when min([ReturnDeliveryFactory_Approved]) = max([ReturnDeliveryFactory_Approved]) then min([ReturnDeliveryFactory_Approved]) else null end as [ReturnDeliveryFactory_Approved]");
        }

        [TestCase("StandardHandling", "[StandardHandling] = coalesce(t.[StandardHandling], c.[StandardHandling])")]
        [TestCase("ExpressDelivery", "[ExpressDelivery] = coalesce(t.[ExpressDelivery], c.[ExpressDelivery])")]
        [TestCase("TaxiCourierDelivery_Approved", "[TaxiCourierDelivery_Approved] = coalesce(t.[TaxiCourierDelivery_Approved], c.[TaxiCourierDelivery_Approved])")]
        public void WriteSetFieldTest(string f, string expected)
        {
            GenerationEnvironment.Clear();
            base.WriteSetField(f);
            Assert.AreEqual(expected, GenerationEnvironment.ToString());
        }

        [TestCase]
        public void WriteSetFieldsTest()
        {
            this.updateFields = GetFields();
            GenerationEnvironment.Clear();
            base.WriteSetFields();

            var sql = GenerationEnvironment.ToString();

            sql.Has("[StandardHandling] = coalesce(t.[StandardHandling], c.[StandardHandling])");
            sql.Has(", [StandardHandling_Approved] = coalesce(t.[StandardHandling_Approved], c.[StandardHandling_Approved])");
            sql.Has(", [HighAvailabilityHandling] = coalesce(t.[HighAvailabilityHandling], c.[HighAvailabilityHandling])");
            sql.Has(", [HighAvailabilityHandling_Approved] = coalesce(t.[HighAvailabilityHandling_Approved], c.[HighAvailabilityHandling_Approved])");
            sql.Has(", [StandardDelivery] = coalesce(t.[StandardDelivery], c.[StandardDelivery])");
            sql.Has(", [StandardDelivery_Approved] = coalesce(t.[StandardDelivery_Approved], c.[StandardDelivery_Approved])");
            sql.Has(", [ExpressDelivery] = coalesce(t.[ExpressDelivery], c.[ExpressDelivery])");
            sql.Has(", [ExpressDelivery_Approved] = coalesce(t.[ExpressDelivery_Approved], c.[ExpressDelivery_Approved])");
            sql.Has(", [TaxiCourierDelivery] = coalesce(t.[TaxiCourierDelivery], c.[TaxiCourierDelivery])");
            sql.Has(", [TaxiCourierDelivery_Approved] = coalesce(t.[TaxiCourierDelivery_Approved], c.[TaxiCourierDelivery_Approved])");
            sql.Has(", [ReturnDeliveryFactory] = coalesce(t.[ReturnDeliveryFactory], c.[ReturnDeliveryFactory])");
            sql.Has(", [ReturnDeliveryFactory_Approved] = coalesce(t.[ReturnDeliveryFactory_Approved], c.[ReturnDeliveryFactory_Approved])");

        }

        private static string[] GetFields()
        {
            return new string[] {
                  "StandardHandling"
                , "HighAvailabilityHandling"
                , "StandardDelivery"
                , "ExpressDelivery"
                , "TaxiCourierDelivery"
                , "ReturnDeliveryFactory"
            };
        }
    }
}
