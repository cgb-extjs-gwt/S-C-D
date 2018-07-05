using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.TestData.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.DataAccessLayer.TestData
{
    public class Module : IModule
    {
        public void Init(IServiceCollection services)
        {
            services.AddTransient<IConfigureDatabaseHandler, TestDataCreationHandlercs>();
        }
    }
}
