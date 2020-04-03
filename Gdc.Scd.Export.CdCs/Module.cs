using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.CdCsJob.Procedures;
using Gdc.Scd.Spooler.Core;

namespace Gdc.Scd.Export.CdCsJob
{
    public class Module : BaseJobModule<Module>
    {
        public override void Load()
        {
            Bind<ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();

            Bind(typeof(GetServiceCostsBySla)).ToSelf();
            Bind(typeof(GetProActiveCosts)).ToSelf();
            Bind(typeof(GetHddRetentionCosts)).ToSelf();
            Bind(typeof(ConfigHandler)).ToSelf();
        }
    }
}
