using Gdc.Scd.Core.Interfaces;
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
        }
    }
}
