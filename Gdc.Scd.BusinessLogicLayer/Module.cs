using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.CapabilityMatrix;
using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.DataAccessLayer.Helpers;
using Ninject.Modules;
using Ninject.Web.Common;

namespace Gdc.Scd.BusinessLogicLayer
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IDomainService<>)).To(typeof(DomainService<>)).InRequestScope();
            Bind<ICostEditorService>().To<CostEditorService>().InRequestScope();
            Bind<ICapabilityMatrixService>().To<CapabilityMatrixService>().InRequestScope();
            Bind<ICalculationService>().To<CalculationService>().InRequestScope();
            Bind<IReportService>().To<ReportService>().InRequestScope();
            Bind<IUserService>().To<UserService>().InRequestScope();
            Bind<ICostBlockHistoryService>().To<CostBlockHistoryService>().InRequestScope();
            Bind<IAvailabilityFeeAdminService>().To<AvailabilityFeeAdminService>().InRequestScope();
            Bind<IEmailService>().To<EmailService>().InRequestScope();
            Bind<ICostBlockFilterBuilder>().To<CostBlockFilterBuilder>().InRequestScope();
            Bind<IQualityGateSevice>().To<QualityGateSevice>().InRequestScope();
            Bind<IActiveDirectoryService>().To<ActiveDirectoryService>().InRequestScope();

            /*----------dictionaries-----------*/
            Kernel.RegisterEntity<ClusterRegion>();
            Kernel.RegisterEntity<Region>();
            Kernel.RegisterEntity<Country>();
            Kernel.RegisterEntity<CountryGroup>();
            Kernel.RegisterEntity<Pla>();
            Kernel.RegisterEntity<Wg>();
            Kernel.RegisterEntity<Availability>();
            Kernel.RegisterEntity<Year>();
            Kernel.RegisterEntity<Duration>();
            Kernel.RegisterEntity<ReactionType>();
            Kernel.RegisterEntity<ReactionTime>();
            Kernel.RegisterEntity<ReactionTimeType>();
            Kernel.RegisterEntity<ReactionTimeAvalability>();
            Kernel.RegisterEntity<ReactionTimeTypeAvalability>();
            Kernel.RegisterEntity<ServiceLocation>();
            Kernel.RegisterEntity<Currency>();
            Kernel.RegisterEntity<ExchangeRate>();
            Kernel.RegisterEntity<YearAvailability>();
            Kernel.RegisterEntity<ClusterPla>();
            Kernel.RegisterEntity<ProActiveSla>();
            Kernel.RegisterEntity<SwDigit>();
            Kernel.RegisterEntity<Sog>();
            Kernel.RegisterEntity<SFab>();
            Kernel.RegisterEntity<SwLicense>();
            Kernel.RegisterEntity<SwDigitLicense>();
            Kernel.RegisterEntity<HwFspCodeTranslation>();
            Kernel.RegisterEntity<SwFspCodeTranslation>();
            Kernel.RegisterEntity<FspCodeTranslation>();

            /*----------admin---------*/
            Kernel.RegisterEntity<AdminAvailabilityFee>();
            Kernel.RegisterEntity<RoleCode>();
            Kernel.RegisterEntity<Role>();
            Kernel.RegisterEntity<UserRole>();

            /*---------domain business logic------------*/
            Kernel.RegisterEntity<CapabilityMatrix>();
            Kernel.RegisterEntity<CapabilityMatrixRule>();
            Kernel.RegisterEntity<HardwareCalculationResult>();
            Kernel.RegisterEntity<SoftwareCalculationResult>();

            /*---------reports----------*/
            Kernel.RegisterEntity<Report>();
            Kernel.RegisterEntity<ReportColumn>();
            Kernel.RegisterEntity<ReportFilter>();

        }
    }
}
