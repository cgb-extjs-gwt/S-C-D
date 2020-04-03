using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.ArchiveJob.Impl;
using Gdc.Scd.Import.Core.Impl;
using Gdc.Scd.Spooler.Core;

namespace Gdc.Scd.Export.Archive
{
    public class Module : BaseJobModule<Module>
    {
        public override void Load()
        {
            Bind<ILogger>().To<Logger>().InSingletonScope();
            Bind<IArchiveRepository>().To<ArchiveRepository>().InTransientScope();
        }
    }
}
