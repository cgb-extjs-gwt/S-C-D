using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.BusinessLogicLayer
{
    public class CalcDetailServiceTest
    {
        public const string RESULT_PATH = "Results";

        private CalcDetailService testing;

        [SetUp]
        public void Setup()
        {
            testing = new CalcDetailService();
        }

        [TestCase]
        public void GetHwCostDetailsTest()
        {
            var d = testing.GetHwCostDetails(false, -1, null).Result;
            //
            Save(d, "service-tp.json");
        }

        public static void Save(object data, string fn)
        {
            StreamUtil.Save(RESULT_PATH, fn, data.AsJson());
        }
    }
}
