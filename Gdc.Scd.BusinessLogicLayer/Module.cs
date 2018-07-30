using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Scd.BusinessLogicLayer
{
    public class Module : IModule
    {
        public void Init(IServiceCollection services)
        {
            services.AddScoped(typeof(IDomainService<>), typeof(DomainService<>));
            services.AddScoped<ICostEditorService, CostEditorService>();
            services.AddScoped<ICostBlockHistoryService, CostBlockHistoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICostBlockHistoryService, CostBlockHistoryService>();

            services.RegisterEntity<Country>();
            services.RegisterEntity<Pla>();
            services.RegisterEntity<Wg>();
        }
    }
}
