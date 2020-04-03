using Gdc.Scd.Export.ArchiveResultSenderJob.Abstract;
using Gdc.Scd.Export.ArchiveResultSenderJob.Concrete;
using Gdc.Scd.Spooler.Core;

namespace Gdc.Scd.Export.ArchiveResultSenderJob
{
    public class Module : BaseJobModule<Module>
    {
        public override void Load()
        {
            Bind<Gdc.Scd.Core.Interfaces.ILogger>().To<Import.Core.Impl.Logger>().InSingletonScope();
            Bind<IArchiveInfoGetter>().To<FileSystemArchiveInfoGetter>().InSingletonScope();
            Bind<ArchiveResultService>().ToSelf();
        }
    }
}
