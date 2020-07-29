using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class CalcHwDetailServiceTest
    {
        public const string RESULT_PATH = "Results";
        private const int ID = 14531504;
        private const bool APPROVED = false;

        private CalcDetailService testing;

        private StandardKernel kernel;

        public CalcHwDetailServiceTest()
        {
            kernel = Module.CreateKernel();
        }

        [SetUp]
        public void Setup()
        {
            testing = kernel.Get<CalcDetailService>();
        }

        [TestCase]
        public void GetStdw_CostDetailsTest()
        {
            var d = testing.GetStdwDetails(APPROVED, 113, 1);
            Save(d, "stdw.json");
        }

        [TestCase]
        public void GetStdw_ById_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "stdw");
            Save(d, "stdw_by_id.json");
        }

        [TestCase]
        public void GetStd_Credit_CostDetailsTest()
        {
            var d = testing.GetStdCreditDetails(APPROVED, 113, 1);
            Save(d, "std-credit.json");
        }

        [TestCase]
        public void GetStd_Credit_ById_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "credit");
            Save(d, "std-credit_by_id.json");
        }

        [TestCase]
        public void GetFieldServiceCost_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "field-service");
            Save(d, "fieldServiceCost.json");
        }

        [TestCase]
        public void GetServiceSupportCost_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "service-support");
            Save(d, "service-support.json");
        }

        [TestCase]
        public void GetLogisticCost_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "logistic");
            Save(d, "logistic.json");
        }

        [TestCase]
        public void GetAvailabilityFee_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "availability-fee");
            Save(d, "availability-fee.json");
        }

        [TestCase]
        public void GetReinsurance_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "reinsurance");
            Save(d, "reinsurance.json");
        }

        [TestCase]
        public void GetOther_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "other");
            Save(d, "other.json");
        }

        [TestCase]
        public void GetMaterial_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "material");
            Save(d, "material.json");
        }

        [TestCase]
        public void GetMaterialOow_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "material-oow");
            Save(d, "material-oow.json");
        }

        [TestCase]
        public void GetTax_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "tax");
            Save(d, "tax.json");
        }

        [TestCase]
        public void GetTaxOow_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "tax-oow");
            Save(d, "tax-oow.json");
        }

        [TestCase]
        public void GetProactive_DetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "proactive");
            Save(d, "proactive.json");
        }

        [TestCase]
        public void GetTP_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "tp");
            Save(d, "service-tp.json");
        }

        [TestCase]
        public void GetTC_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "tc");
            Save(d, "service-tс.json");
        }

        [TestCase]
        public void GetReactiveTC_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "reactive-tc");
            Save(d, "reactive-tc.json");
        }

        [TestCase]
        public void GetReactiveTP_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(APPROVED, ID, "reactive-tp");
            Save(d, "reactive-tp.json");
        }

        public static void Save(object data, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, data.AsJson());
        }
    }
}
