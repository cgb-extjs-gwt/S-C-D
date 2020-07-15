using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class CalcHddRetentionDetailServiceTest
    {
        public const string RESULT_PATH = "Results";

        private CalcDetailService testing;

        private StandardKernel kernel;

        public CalcHddRetentionDetailServiceTest()
        {
            kernel = Module.CreateKernel();
        }

        [SetUp]
        public void Setup()
        {
            testing = kernel.Get<CalcDetailService>();
        }

        [TestCase]
        public void GetHdd_CostDetailsTest()
        {
            var d = testing.GetHddCostDetails(false, 102);
            //
            Save(d, "hw-hdd.json");
        }

        public void Save(object data, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, data.AsJson());
        }
    }
}
