using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class CalcDetailServiceTest
    {
        public const string RESULT_PATH = "Results";

        private CalcDetailService testing;

        private StandardKernel kernel;

        public CalcDetailServiceTest()
        {
            kernel = Module.CreateKernel();
        }

        [SetUp]
        public void Setup()
        {
            testing = kernel.Get<CalcDetailService>();
        }

        [TestCase]
        public void GetFieldServiceCost_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(false, 14531504, "field-service").Result;
            //
            Save(d, "fieldServiceCost.json");
        }

        [TestCase]
        public void GetTP_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(false, 14531504, "tp").Result;
            //
            Save(d, "service-tp.json");
        }

        [TestCase]
        public void GetTC_CostDetailsTest()
        {
            var d = testing.GetHwCostDetails(false, 14531504, "tc").Result;
            //
            Save(d, "service-tс.json");
        }

        public static void Save(object data, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, data.AsJson());
        }
    }
}
