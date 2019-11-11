using System;
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

            var sql = ByPla();

            Assert.True(sql.Contains("('AA1', 'XYZ', 'ABC')"));
            Assert.True(sql.Contains("Pla"), "Pla not found");
            Assert.True(sql.Contains("create index ix_tmp_Country_SLA on #tmp([Country], [Pla], [ReactionTimeType]);"), "index ix_tmp_Country_SLA");
            Assert.True(sql.Contains("create index ix_tmpmin_Country_SLA on #tmpMin([Country], [Pla], [ReactionTimeType]);"), "ix_tmpmin_Country_SLA");

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

        [TestCase("StandardHandling", ", case when min([StandardHandling]) = max([StandardHandling]) then min([StandardHandling]) else null end as [StandardHandling]")]
        [TestCase("HighAvailabilityHandling_Approved", ", case when min([HighAvailabilityHandling_Approved]) = max([HighAvailabilityHandling_Approved]) then min([HighAvailabilityHandling_Approved]) else null end as [HighAvailabilityHandling_Approved]")]
        [TestCase("TaxiCourierDelivery", ", case when min([TaxiCourierDelivery]) = max([TaxiCourierDelivery]) then min([TaxiCourierDelivery]) else null end as [TaxiCourierDelivery]")]
        public void WriteSelectFieldTest(string f, string expected)
        {
            GenerationEnvironment.Clear();
            base.WriteSelectField(f);
            Assert.AreEqual(expected, GenerationEnvironment.ToString());
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
        public void WriteUpdateFieldsTest()
        {
            GenerationEnvironment.Clear();
            var expected = @"StandardHandling = coalesce(t.StandardHandling, c.StandardHandling) 
, HighAvailabilityHandling = coalesce(t.HighAvailabilityHandling, c.HighAvailabilityHandling) 
, StandardDelivery = coalesce(t.StandardDelivery, c.StandardDelivery) 
, ExpressDelivery = coalesce(t.ExpressDelivery, c.ExpressDelivery) 
, TaxiCourierDelivery = coalesce(t.TaxiCourierDelivery, c.TaxiCourierDelivery) 
, ReturnDeliveryFactory = coalesce(t.ReturnDeliveryFactory, c.ReturnDeliveryFactory) 
, StandardHandling_Approved = coalesce(t.StandardHandling_Approved, c.StandardHandling_Approved) 
, HighAvailabilityHandling_Approved = coalesce(t.HighAvailabilityHandling_Approved, c.HighAvailabilityHandling_Approved) 
, StandardDelivery_Approved = coalesce(t.StandardDelivery_Approved, c.StandardDelivery_Approved) 
, ExpressDelivery_Approved = coalesce(t.ExpressDelivery_Approved, c.ExpressDelivery_Approved) 
, TaxiCourierDelivery_Approved = coalesce(t.TaxiCourierDelivery_Approved, c.TaxiCourierDelivery_Approved) 
, ReturnDeliveryFactory_Approved = coalesce(t.ReturnDeliveryFactory_Approved, c.ReturnDeliveryFactory_Approved) ";

            base.WriteUpdateFields();
            Assert.AreEqual(expected, GenerationEnvironment.ToString());
        }
    }
}
