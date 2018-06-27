using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Impl;
using Gdc.Scd.Core.Meta.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.Core
{
    public class Module : IModule
    {
        public void Init(IServiceCollection services)
        {
            services.AddSingleton<IDomainMetaSevice, DomainMetaSevice>();
            services.AddSingleton<IDomainEnitiesMetaService, DomainEnitiesMetaService>();
            services.AddSingleton(serviceProvider => serviceProvider.GetService<IDomainMetaSevice>().Get());
            services.AddSingleton(serviceProvider => 
            {
                var domainMeta = serviceProvider.GetService<DomainMeta>();
                var domainEnitiesMetaService = serviceProvider.GetService<IDomainEnitiesMetaService>();

                return domainEnitiesMetaService.Get(domainMeta);
            });
        }
    }
}
