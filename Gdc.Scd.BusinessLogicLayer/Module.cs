using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Entities.CapabilityMatrix;
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
            services.AddScoped<ICapabilityMatrixService, CapabilityMatrixService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICostBlockHistoryService, CostBlockHistoryService>();
            services.AddScoped<IEmailService, EmailService>();

            services.RegisterEntity<Country>();
            services.RegisterEntity<Pla>();
            services.RegisterEntity<Wg>();
            services.RegisterEntity<Availability>();
            services.RegisterEntity<Duration>();
            services.RegisterEntity<ReactionType>();
            services.RegisterEntity<ReactionTime>();
            services.RegisterEntity<ServiceLocation>();
            services.RegisterEntity<CapabilityMatrix>();
            services.RegisterEntity<CapabilityMatrixRule>();
            services.RegisterEntity<CapabilityMatrixAllowView>();
            services.RegisterEntity<AdminAvailabilityFee>();
            services.RegisterEntity<CapabilityMatrixCountryAllowView>();
        }
    }
}
