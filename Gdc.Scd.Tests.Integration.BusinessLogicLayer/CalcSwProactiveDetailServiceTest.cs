using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class CalcSwProactiveDetailServiceTest
    {
        public const string RESULT_PATH = "Results";

        private CalcDetailService testing;

        private StandardKernel kernel;

        public CalcSwProactiveDetailServiceTest()
        {
            kernel = Module.CreateKernel();
        }

        [SetUp]
        public void Setup()
        {
            testing = kernel.Get<CalcDetailService>();
        }

        [TestCase]
        public void GetCostDetailsTest()
        {
            var d = testing.GetSwProactiveCostDetails(false, 27020, "FSP:G-SW1MD60PRFF0");
            //
            Save(d, "sw-proactive.json");
        }

        public void Save(object data, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, data.AsJson());
        }
    }
}
