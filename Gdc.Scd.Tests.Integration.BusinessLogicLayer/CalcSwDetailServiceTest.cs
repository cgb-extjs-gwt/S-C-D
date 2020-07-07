using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class CalcSwDetailServiceTest
    {
        public const string RESULT_PATH = "Results";

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
            var d = testing.GetSwCostDetails(false, 657, "service-support");
            //
            Save(d, "sw-service-support.json");
        }

        [TestCase]
        public void GetReinsurance_CostDetailsTest()
        {
            var d = testing.GetSwCostDetails(false, 657, "reinsurance");
            //
            Save(d, "sw-Reinsurance.json");
        }

        public void Save(object data, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, data.AsJson());
        }
    }
}
