using Gdc.Scd.Import.Por.Core.Scripts;
using Gdc.Scd.Tests.Integration.Import.Por.Helpers;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Import.Por
{
    public class UpdateSwSpMaintenanceTest
    {
        [TestCase]
        public void SqlBySogTest()
        {
            var wgs = InputAtomHelper.CreateDigit("aa1", "xyz", "abc");
            var tpl = new UpdateSwSpMaintenance(wgs);
            var sql = tpl.BySog();

            sql.Has("insert into @dig(id) select id from InputAtoms.SwDigit where Deactivated = 0 and UPPER(name) in ('AA1', 'XYZ', 'ABC')");
            sql.Has("Sog", "Sog not found");
            sql.Has("[DurationAvailability]", "[DurationAvailability] not found");
        }
    }
}
