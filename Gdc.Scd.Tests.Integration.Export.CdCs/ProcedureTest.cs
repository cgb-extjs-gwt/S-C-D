using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.CdCs.Procedures;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.CdCs
{
    public class ProcedureTest
    {
        private const string COUNTRY = "Germany";

        private IKernel kernel;

        IRepositorySet repo;

        public ProcedureTest()
        {
            kernel = Gdc.Scd.Export.CdCs.Module.CreateKernel();
            repo = kernel.Get<IRepositorySet>();
        }

        [TestCase]
        public void GetHddRetentionCostsTest()
        {
            var proc = new GetHddRetentionCosts(repo);
            Assert.NotZero(proc.Execute(COUNTRY).Count);
        }

        [TestCase]
        public void GetProActiveCostsTest()
        {
            var proc = new GetProActiveCosts(new CommonService(repo));
            Assert.NotZero(proc.Execute(COUNTRY).Count);
        }

        [TestCase]
        public void GetServiceCostsBySlaTest()
        {
            var proc = new GetServiceCostsBySla(repo);
            var sla = new CdCsServiceTest().ReadSla();

            Assert.NotZero(proc.Execute(COUNTRY, sla).Count);
        }
    }
}
