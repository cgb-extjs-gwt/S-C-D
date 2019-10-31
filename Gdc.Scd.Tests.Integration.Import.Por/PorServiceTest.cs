using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por;
using Gdc.Scd.Tests.Integration.Import.Por.Testings;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;
using System.Collections.Generic;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class PorServiceTest : PorService
    {
        private FakeLogger fakeLogger;

        private FakeCostBlockUpdateService fakeCostBlockUpdateService;

        [SetUp]
        public void Setup()
        {
            fakeLogger = new FakeLogger();
            fakeCostBlockUpdateService = new FakeCostBlockUpdateService();
            this.Logger = fakeLogger;
            this.CostBlockUpdateService = fakeCostBlockUpdateService;
        }

        [TestCase]
        public void UpdateCostBlocksByPlaTest()
        {
            this.fakeCostBlockUpdateService.OnUpdateByPla = () =>
            {
                Assert.IsTrue(fakeLogger.IsInfo);
                Assert.AreEqual("STEP -1: Updating cost block by pla started...", fakeLogger.Message);
            };

            this.UpdateCostBlocksByPla(-1, new List<Wg>(0));
            Assert.IsTrue(fakeLogger.IsInfo);
            Assert.AreEqual("Cost block by pla updated.", fakeLogger.Message);
        }

        [TestCase]
        public void UpdateCostBlocksByPlaShouldLogErrorTest()
        {
            fakeCostBlockUpdateService.error = new System.Exception("Error here!");
            this.UpdateCostBlocksByPla(-1, null);
            Assert.IsTrue(fakeLogger.IsError);
            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
        }

        [TestCase]
        public void UpdateCostBlocksBySogTest()
        {
            this.fakeCostBlockUpdateService.OnUpdateByPla = () =>
            {
                Assert.IsTrue(fakeLogger.IsInfo);
                Assert.AreEqual("STEP -999: Updating software cost block by sog started...", fakeLogger.Message);
            };

            this.UpdateCostBlocksBySog(-999, new List<SwDigit>(0));
            Assert.IsTrue(fakeLogger.IsInfo);
            Assert.AreEqual("Software cost block by sog updated.", fakeLogger.Message);
        }

        [TestCase]
        public void UpdateCostBlocksBySogShouldLogErrorTest()
        {
            fakeCostBlockUpdateService.error = new System.Exception("Error here!");
            this.UpdateCostBlocksBySog(-1, new List<SwDigit>(0));
            Assert.IsTrue(fakeLogger.IsError);
            Assert.AreEqual("POR Import completed unsuccessfully. Please find details below.", fakeLogger.Message);
        }
    }
}
