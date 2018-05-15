using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.BusinessLogicLayer
{
    public class Module : IModule
    {
        public void Init(IServiceCollection services)
        {
            services.AddSingleton(typeof(IDomainService<>), typeof(DomainService<>));
        }
    }
}
