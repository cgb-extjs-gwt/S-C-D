using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class CalcSwDetailServiceTest
    {
        public const string RESULT_PATH = "Results";
        
        private const int ROW_ID = 657;
        
        private const bool APPROVED = false;
        
        private CalcDetailService testing;

        private StandardKernel kernel;

        public CalcSwDetailServiceTest()
        {
            kernel = Module.CreateKernel();
        }

        [SetUp]
        public void Setup()
        {
            testing = kernel.Get<CalcDetailService>();
        }

        [TestCase]
        public void GetServiceSupport_CostDetailsTest()
        {
            var d = testing.GetSwCostDetails(APPROVED, ROW_ID, "service-support");
            Save(d, "sw-service-support.json");
        }

        [TestCase]
        public void GetReinsurance_CostDetailsTest()
        {
            var d = testing.GetSwCostDetails(APPROVED, ROW_ID, "reinsurance");
            Save(d, "sw-Reinsurance.json");
        }

        [TestCase]
        public void GetTransfer_CostDetailsTest()
        {
            var d = testing.GetSwCostDetails(APPROVED, ROW_ID, "transfer");
            Save(d, "sw-transfer.json");
        }

        [TestCase]
        public void GetMaintenance_CostDetailsTest()
        {
            var d = testing.GetSwCostDetails(APPROVED, ROW_ID, "maintenance");
            Save(d, "sw-maintenance.json");
        }

        [TestCase]
        public void GetDealer_CostDetailsTest()
        {
            var d = testing.GetSwCostDetails(APPROVED, ROW_ID, "dealer");
            Save(d, "sw-dealer.json");
        }

        public void Save(object data, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, data.AsJson());
        }
    }
}
