using Gdc.Scd.Import.Por.Core.Scripts;
using Gdc.Scd.Tests.Integration.Import.Por.Helpers;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class UpdateFieldServiceCostTest
    {
        [TestCase]
        public void SqlByCentralContractGroupTest()
        {
            var wgs = InputAtomHelper.CreateWg("aa1", "xyz", "abc");
            var tpl = new UpdateFieldServiceCost(wgs);
            var sql = tpl.ByCentralContractGroup();

            sql.Has("('AA1', 'XYZ', 'ABC')");
            sql.Has("CentralContractGroup", "CentralContractGroup not found");
            sql.Has("group by [Country], [CentralContractGroup], [ServiceLocation], [ReactionTimeType];");
        }

        [TestCase]
        public void SqlByPlaTest()
        {
            var wgs = InputAtomHelper.CreateWg("xxx", "yyy", "zzz", "pxy");
            var tpl = new UpdateFieldServiceCost(wgs);
            var sql = tpl.ByPla();

            sql.Has("('XXX', 'YYY', 'ZZZ', 'PXY')");
            sql.Has("Pla", "pla not found");
            sql.Has("group by [Country], [Pla], [ServiceLocation], [ReactionTimeType];");
        }
    }
}
