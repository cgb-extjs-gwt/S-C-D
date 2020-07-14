if OBJECT_ID('SoftwareSolution.SpGetProactiveCostDetailsByID') is not null
    drop procedure [SoftwareSolution].SpGetProactiveCostDetailsByID;
go

create procedure [SoftwareSolution].SpGetProactiveCostDetailsByID(
    @approved       bit, 
    @id             bigint,
    @fsp            nvarchar(32)
)
as
begin

    declare @country nvarchar(64);
    declare @central nvarchar(32) = 'Central';
    declare @matrix nvarchar(32) = 'Repetition matrix';

    --#### Pro active #####################################################################################

    declare @proID bigint = (select ProactiveSlaId from Fsp.SwFspCodeTranslation where Name = @fsp);
    declare @pro nvarchar(64);

    declare @LocalRemoteAccessSetupPreparationEffort float;
    declare @LocalRegularUpdateReadyEffort float;
    declare @LocalPreparationShcEffort float;
    declare @LocalRemoteShcCustomerBriefingEffort float;
    declare @LocalOnSiteShcCustomerBriefingEffort float;
    declare @TravellingTime float;
    declare @OnSiteHourlyRate float;
    declare @CentralExecutionShcReportCost float;
    declare @CentralExecutionShcReportRepetition int;
    declare @LocalOnsiteShcCustomerBriefingRepetition int;
    declare @LocalPreparationShcRepetition int;
    declare @LocalRegularUpdateReadyRepetition int;
    declare @LocalRemoteShcCustomerBriefingRepetition int;
    declare @TravellingTimeRepetition int;

    declare @tbl table (
          Mandatory       bit default(1)
        , CostBlock       nvarchar(64)
        , CostElement     nvarchar(64)
        , Value           nvarchar(64)
        , Dependency      nvarchar(64)
        , Level           nvarchar(64)
        , [order]         int identity
    );

    select     @country = (select Name from InputAtoms.Country where id = pro.Country)
             , @CentralExecutionShcReportCost = case when @approved = 0 then CentralExecutionShcReportCost else CentralExecutionShcReportCost_Approved end
             , @LocalRemoteAccessSetupPreparationEffort  = case when @approved = 0 then LocalRemoteAccessSetupPreparationEffort  else LocalRemoteAccessSetupPreparationEffort_Approved end
             , @LocalRegularUpdateReadyEffort  = case when @approved = 0 then LocalRegularUpdateReadyEffort  else LocalRegularUpdateReadyEffort_Approved end
             , @LocalPreparationShcEffort  = case when @approved = 0 then LocalPreparationShcEffort  else LocalPreparationShcEffort_Approved end
             , @LocalRemoteShcCustomerBriefingEffort  = case when @approved = 0 then LocalRemoteShcCustomerBriefingEffort  else LocalRemoteShcCustomerBriefingEffort_Approved end
             , @LocalOnSiteShcCustomerBriefingEffort  = case when @approved = 0 then LocalOnSiteShcCustomerBriefingEffort  else LocalOnSiteShcCustomerBriefingEffort_Approved end
             , @TravellingTime  = case when @approved = 0 then TravellingTime  else TravellingTime_Approved end
             , @OnSiteHourlyRate  = case when @approved = 0 then OnSiteHourlyRate else OnSiteHourlyRate_Approved end
    from SoftwareSolution.ProActiveSw pro
    where pro.Id = @id

    select   @pro = ExternalName
           , @CentralExecutionShcReportRepetition  = CentralExecutionShcReportRepetition 
           , @LocalOnsiteShcCustomerBriefingRepetition  = LocalOnsiteShcCustomerBriefingRepetition 
           , @LocalPreparationShcRepetition  = LocalPreparationShcRepetition 
           , @LocalRegularUpdateReadyRepetition  = LocalRegularUpdateReadyRepetition 
           , @LocalRemoteShcCustomerBriefingRepetition  = LocalRemoteShcCustomerBriefingRepetition 
           , @TravellingTimeRepetition = TravellingTimeRepetition 
    from Dependencies.ProActiveSla where Id = @proID;

    insert into @tbl values
          (1, 'ProActive', 'Local Remote-Access setup preparation effort', FORMAT(@LocalRemoteAccessSetupPreparationEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local regular update ready for service effort', FORMAT(@LocalRegularUpdateReadyEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local preparation SHC effort', FORMAT(@LocalPreparationShcEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Central execution SHC & report cost', FORMAT(@CentralExecutionShcReportCost, '') + ' EUR', null, @central)
        , (1, 'ProActive', 'Local remote SHC customer briefing effort', FORMAT(@LocalRemoteShcCustomerBriefingEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Local on-site SHC customer briefing effort', FORMAT(@LocalOnSiteShcCustomerBriefingEffort, '') + ' h', null, @country)
        , (1, 'ProActive', 'Travelling Time (MTTT)', FORMAT(@TravellingTime, '') + ' h', null, @country)
        , (1, 'ProActive', 'On-Site Hourly Rate', FORMAT(@OnSiteHourlyRate, '') + ' EUR', null, @country);

    insert into @tbl values
          (1, 'ProActive', 'ProActive SLA', @pro, @fsp, @country)

    if @proID is not null
        insert into @tbl values
              (1, 'ProActive', 'Local regular update ready for service repetition', FORMAT(@LocalRegularUpdateReadyRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Local preparation SHC repetition', FORMAT(@LocalPreparationShcRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Central execution SHC & report repetition', FORMAT(@CentralExecutionShcReportRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Local remote SHC customer briefing repetition', FORMAT(@LocalRemoteShcCustomerBriefingRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Local on-site SHC customer briefing repetition', FORMAT(@LocalOnsiteShcCustomerBriefingRepetition, ''), @pro, @matrix)
            , (1, 'ProActive', 'Travelling Time repetition', FORMAT(@TravellingTimeRepetition, ''), @pro, @matrix)


    --##########################################

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go

exec SoftwareSolution.SpGetProactiveCostDetailsByID 0, 12478, 'FSP:G-SW1MD60PRFF0';