using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.CdCsJob.Procedures;
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
            kernel = Gdc.Scd.Export.CdCsJob.Module.CreateKernel();
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
            var sla = ExcelWriterTest.GenSlaCollection();

            var data = proc.Execute(113, sla);

            Assert.NotZero(data.Count);

            foreach(var row in data)
            {
                Assert.AreEqual("Germany", row.CountryGroup);
                AssertString(row.Key);
                AssertString(row.ServiceLocation);
                AssertString(row.Availability);
                AssertString(row.ReactionTime);
                AssertString(row.ReactionType);
                AssertString(row.WarrantyGroup);
                AssertString(row.Duration);
            }
        }

        public static void AssertString(string s)
        {
            Assert.False(string.IsNullOrEmpty(s));
        }
    }
}
