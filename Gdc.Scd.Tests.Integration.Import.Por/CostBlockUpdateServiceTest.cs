using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Impl;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class CostBlockUpdateServiceTest
    {
        private IKernel kernel;

        private IRepositorySet repo;

        private CostBlockUpdateService testing;

        public CostBlockUpdateServiceTest()
        {
            kernel = Gdc.Scd.Import.Por.Module.CreateKernel();
            repo = kernel.Get<IRepositorySet>();
        }

        [SetUp]
        public void Setup()
        {
            this.testing = new CostBlockUpdateService(repo);
        }

        [TestCase]
        public void UpdateByPlaTest()
        {
            testing.UpdateByPla(CreateWg());
        }

        [TestCase]
        public void UpdateFieldServiceCostTest()
        {
            testing.UpdateFieldServiceCost(CreateWg());
        }

        [TestCase]
        public void UpdateLogisticsCostTest()
        {
            testing.UpdateLogisticsCost(CreateWg());
        }

        [TestCase]
        public void UpdateMarkupOtherCostsTest()
        {
            testing.UpdateMarkupOtherCosts(CreateWg());
        }

        [TestCase]
        public void UpdateMarkupStandardWarantyTest()
        {
            testing.UpdateMarkupStandardWaranty(CreateWg());
        }

        [TestCase]
        public void UpdateProActiveCostsTest()
        {
            testing.UpdateProactive(CreateWg());
        }

        [TestCase]
        public void UpdateSwSpMaintenanceTest()
        {
            testing.UpdateSwSpMaintenance(CreateDigit());
        }

        private Wg[] CreateWg()
        {
            return new Wg[] {
                new Wg { Name = "TC4" },
                new Wg { Name = "S40" },
                new Wg { Name = "S55" },
                new Wg { Name = "VSH" },
                new Wg { Name = "MN1" },
                new Wg { Name = "MN4" }
            };
        }

        private SwDigit[] CreateDigit()
        {
            return new SwDigit[]
            {
                  new SwDigit { Name = "E9" }
                , new SwDigit { Name = "GX" }
                , new SwDigit { Name = "HR" }
                , new SwDigit { Name = "LQ" }
                , new SwDigit { Name = "LR" }
            };
        }
    }
}
