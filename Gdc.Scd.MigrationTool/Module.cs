using System.Reflection;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.MigrationTool.Entities;
using Gdc.Scd.MigrationTool.Impl;
using Gdc.Scd.MigrationTool.Interfaces;
using Ninject.Modules;

namespace Gdc.Scd.MigrationTool
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            this.AutoRegistrationMigrations();

            this.Bind<IMigrationService>().To<MigrationService>().InTransientScope();

            this.Kernel.RegisterEntityAsUnique<Migration>(nameof(Migration.Number));
        }

        private void AutoRegistrationMigrations()
        {
            var interfaceMigrationType = typeof(IMigrationAction);

            foreach (var migrationType in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!migrationType.IsInterface && interfaceMigrationType.IsAssignableFrom(migrationType))
                {
                    this.Bind(interfaceMigrationType).To(migrationType).InTransientScope();
                }
            }
        }
    }
}
