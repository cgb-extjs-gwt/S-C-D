using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.Scripts;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class UpdateMarkupOtherCostsTest
    {
        [TestCase]
        public void SqlByCentralContractGroupTest()
        {
            var wgs = CreateWg("aa1", "xyz", "abc");
            var tpl = new UpdateMarkupOtherCosts(wgs);
            var sql = tpl.ByCentralContractGroup();

            Assert.True(sql.Contains("('AA1', 'XYZ', 'ABC')"));
            Assert.True(sql.Contains("CentralContractGroup"), "CentralContractGroup not found");
        }

        [TestCase]
        public void SqlByPlaTest()
        {
            var wgs = CreateWg("xxx", "yyy", "zzz", "pxy");
            var tpl = new UpdateMarkupOtherCosts(wgs);
            var sql = tpl.ByPla();

            Assert.True(sql.Contains("('XXX', 'YYY', 'ZZZ', 'PXY')"));
            Assert.True(sql.Contains("Pla"), "pla not found");
        }

        private Wg[] CreateWg(params string[] names)
        {
            if(names.Length == 0)
            {
                throw new System.ArgumentException();
            }

            var arr = new Wg[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                arr[i] = new Wg { Name = names[i] };
            }
            return arr;
        }
    }
}
