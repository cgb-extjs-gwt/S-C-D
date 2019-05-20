using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.ArchiveResultSender.Abstract;
using Gdc.Scd.Export.ArchiveResultSender.Concrete;
using Ninject.Modules;
using NLog;

namespace Gdc.Scd.Export.ArchiveResultSender
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ILogger<LogLevel>>().To<Import.Core.Impl.Logger>().InSingletonScope();
            Bind<IArchiveInfoGetter>().To<FileSystemArchiveInfoGetter>().InSingletonScope();
            Bind<IEmailService>().To<EmailService>().InSingletonScope();
        }
    }
}
