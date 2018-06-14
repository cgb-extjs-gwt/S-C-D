using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Meta.Impl;
using Gdc.Scd.BusinessLogicLayer.Meta.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.BusinessLogicLayer
{
    public class Module : IModule
    {
        public void Init(IServiceCollection services)
        {
            services.AddScoped(typeof(IDomainService<>), typeof(DomainService<>));
            services.AddSingleton<IDomainMetaSevice, DomainMetaSevice>();
            services.AddScoped<ICostEditorService, CostEditorService>();
            services.AddScoped<ICountryService, CountryService>();
        }
    }
}
