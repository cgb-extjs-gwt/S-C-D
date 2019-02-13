using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Calculation;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.DataAccessLayer.Helpers;
using Ninject.Modules;

namespace Gdc.Scd.BusinessLogicLayer
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind(typeof(IDomainService<>)).To(typeof(DomainService<>)).InScdRequestScope();
            Bind<IWgService>().To<WgService>().InScdRequestScope();
            Bind<IWgPorService>().To<WgPorDecoratorService>().InScdRequestScope();
            Bind<ICostEditorService>().To<CostEditorService>().InScdRequestScope();
            Bind<IPortfolioService>().To<PortfolioService>().InScdRequestScope();
            Bind<ICalculationService>().To<CalculationService>().InScdRequestScope();
            Bind<IHddRetentionService>().To<HddRetentionService>().InScdRequestScope();
            Bind<IReportService>().To<ReportService>().InScdRequestScope();
            Bind<IUserService>().To<UserService>().InScdRequestScope();
            Bind<ICostBlockHistoryService>().To<CostBlockHistoryService>().InScdRequestScope();
            Bind<IAvailabilityFeeAdminService>().To<AvailabilityFeeAdminService>().InScdRequestScope();
            Bind<ICountryAdminService>().To<CountryAdminService>().InScdRequestScope();
            Bind<ICountryUserService>().To<CountryUserService>().InScdRequestScope();
            Bind<IEmailService>().To<EmailService>().InScdRequestScope();
            Bind<IQualityGateSevice>().To<QualityGateSevice>().InScdRequestScope();
            Bind<IActiveDirectoryService>().To<ActiveDirectoryService>().InScdRequestScope();
            Bind<ITableViewService>().To<TableViewService>().InScdRequestScope();
            Bind<IAppService>().To<AppService>().InScdRequestScope();
            Bind<ICostBlockService>().To<CostBlockService>().InScdRequestScope();
            Bind<IApprovalService>().To<ApprovalService>().InScdRequestScope();
            Bind<INotifyChannel>().To<MemoryChannel>().InSingletonScope();
            Bind<ICostImportExcelService>().To<CostImportExcelService>().InScdRequestScope();
            Bind<IRoleCodeService>().To<RoleCodeService>().InScdRequestScope();

            /*----------dictionaries-----------*/
            Kernel.RegisterEntity<ClusterRegion>();
            Kernel.RegisterEntity<Region>();
            Kernel.RegisterEntity<Country>();
            Kernel.RegisterEntity<CountryGroup>();
            Kernel.RegisterEntity<Pla>();
            Kernel.RegisterEntity<CentralContractGroup>();
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
            Kernel.RegisterEntity<DurationAvailability>();
            Kernel.RegisterEntity<ClusterPla>();
            Kernel.RegisterEntity<ProActiveSla>();
            Kernel.RegisterEntity<SwDigit>();
            Kernel.RegisterEntity<Sog>();
            Kernel.RegisterEntity<SFab>();
            Kernel.RegisterEntity<SwLicense>();
            Kernel.RegisterEntity<SwDigitLicense>();
            Kernel.RegisterEntity<HwFspCodeTranslation>();
            Kernel.RegisterEntity<HwHddFspCodeTranslation>();
            Kernel.RegisterEntity<SwFspCodeTranslation>();
            Kernel.RegisterEntity<ImportConfiguration>();
            Kernel.RegisterEntity<ProActiveDigit>();
            /*----------cost block entities---------*/
            Kernel.RegisterEntity<AvailabilityFee>();
            Kernel.RegisterEntity<TaxAndDutiesEntity>();
            Kernel.RegisterEntity<Afr>();
            Kernel.RegisterEntity<InstallBase>();
            Kernel.RegisterEntity<MaterialCostInWarranty>();
            Kernel.RegisterEntity<CdCsConfiguration>();
            /*----------admin---------*/
            Kernel.RegisterEntity<AdminAvailabilityFee>();
            Kernel.RegisterEntity<RoleCode>();

            /*---------domain business logic------------*/
            Kernel.RegisterEntity<LocalPortfolio>();
            Kernel.RegisterEntity<PrincipalPortfolio>();
            Kernel.RegisterEntity<HardwareManualCost>();
            Kernel.RegisterEntity<HddRetentionManualCost>();
            Kernel.RegisterEntity<HddRetentionView>();

            /*---------reports----------*/
            Kernel.RegisterEntity<Report>();
            Kernel.RegisterEntity<ReportColumn>();
            Kernel.RegisterEntity<ReportFilter>();
            Kernel.RegisterEntity<JobsSchedule>();
        }
    }
}
