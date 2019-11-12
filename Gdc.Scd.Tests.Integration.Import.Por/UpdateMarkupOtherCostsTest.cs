using Gdc.Scd.Import.Por.Core.Scripts;
using Gdc.Scd.Tests.Integration.Import.Por.Helpers;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class UpdateMarkupOtherCostsTest
    {
        [TestCase]
        public void SqlByCentralContractGroupTest()
        {
            var wgs = InputAtomHelper.CreateWg("aa1", "xyz", "abc");
            var tpl = new UpdateMarkupOtherCosts(wgs);
            var sql = tpl.ByCentralContractGroup();

            sql.Has("('AA1', 'XYZ', 'ABC')");
            sql.Has("CentralContractGroup", "CentralContractGroup not found");
        }

        [TestCase]
        public void SqlByPlaTest()
        {
            var wgs = InputAtomHelper.CreateWg("xxx", "yyy", "zzz", "pxy");
            var tpl = new UpdateMarkupOtherCosts(wgs);
            var sql = tpl.ByPla();

            sql.Has("('XXX', 'YYY', 'ZZZ', 'PXY')");
            sql.Has("Pla", "pla not found");
        }
    }
}
