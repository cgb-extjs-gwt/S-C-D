declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SW-PARAM-OVERVIEW');
if @reportId is null 
begin

    insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) values
    ('SW-PARAM-OVERVIEW', 'Software & Solution calculation parameter overview', 1,  1, 'Report.SwParamOverview');

end

go

IF OBJECT_ID('Report.SwParamOverview') IS NOT NULL
  DROP FUNCTION Report.SwParamOverview;
go 

CREATE FUNCTION [Report].[SwParamOverview]
(
    @cnt bigint,
    @sog bigint,
    @digit dbo.ListID readonly,
    @av dbo.ListID readonly,
    @year dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    with GermanyServiceCte as (
        SELECT   ssc.ClusterPla
               , ssc.[1stLevelSupportCostsCountry_Approved] / er.Value as [1stLevelSupportCosts]
               , ssc.TotalIb_Approved as TotalIb

        FROM Hardware.ServiceSupportCost ssc
        JOIN InputAtoms.Country c on c.Id = ssc.Country and c.ISO3CountryCode = 'DEU' --install base by Germany!
        LEFT JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
    )
    , ProCte as (
        select   pro.Country
               , pro.SwDigit
               , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteAccessSetupPreparationEffort
               , pro.LocalRegularUpdateReadyEffort_Approved as LocalRegularUpdateReadyEffort
               , pro.LocalPreparationShcEffort_Approved as LocalPreparationShcEffort
               , pro.CentralExecutionShcReportCost_Approved as CentralExecutionShcReportCost
               , pro.LocalRemoteShcCustomerBriefingEffort_Approved as LocalRemoteShcCustomerBriefingEffort
               , pro.LocalOnSiteShcCustomerBriefingEffort_Approved as LocalOnSiteShcCustomerBriefingEffort
               , pro.TravellingTime_Approved as TravellingTime
               , pro.OnSiteHourlyRate_Approved as OnSiteHourlyRate

        from SoftwareSolution.ProActiveSw pro
        where    pro.Country = @cnt 
             and pro.DeactivatedDateTime is null
    )
    select   c.Name as Country
           , sog.Description as SogDescription
           , sog.Name as Sog
           , dig.Name as Digit
           , null as SwProduct
           , dig.Description as DigitDescription

           , av.Name as Availability
           , case when dur.IsProlongation = 0 then CAST(dur.Value as nvarchar(16)) else 'prolongation' end as Duration

           , fsp.Name as Fsp
           , fsp.ShortDescription FspDescription

           , ssc.[1stLevelSupportCosts] as [1stLevelSupportCosts]
           , m.[2ndLevelSupportCosts_Approved] as [2ndLevelSupportCosts]
       
           , ssc.TotalIb as TotalIb
           , sum(m.InstalledBaseSog_Approved) over(partition by m.Sfab) as IB_SFAB

           , cur.Name as CurrencyReinsurance
           , m.ReinsuranceFlatfee_Approved as ReinsuranceFlatfee

           , m.ShareSwSpMaintenanceListPrice_Approved as ShareSwSpMaintenanceListPrice
           , m.RecommendedSwSpMaintenanceListPrice_Approved as RecommendedSwSpMaintenanceListPrice
           , m.DiscountDealerPrice_Approved as DiscountDealerPrice
           , m.MarkupForProductMarginSwLicenseListPrice_Approved as MarkupForProductMarginSwLicenseListPrice

           , pro.LocalRemoteAccessSetupPreparationEffort
           , pro.LocalRegularUpdateReadyEffort
           , pro.LocalPreparationShcEffort
           , pro.CentralExecutionShcReportCost
           , pro.LocalRemoteShcCustomerBriefingEffort
           , pro.LocalOnSiteShcCustomerBriefingEffort
           , pro.TravellingTime
           , pro.OnSiteHourlyRate

    from SoftwareSolution.SwSpMaintenance m
    join ProCte pro on pro.SwDigit = m.SwDigit

    join InputAtoms.Pla pla on pla.Id = m.Pla
    left join GermanyServiceCte ssc on ssc.ClusterPla = pla.ClusterPlaId

    join InputAtoms.Country c on c.id = pro.Country
    join InputAtoms.Sog sog on sog.Id = m.Sog
    join InputAtoms.SwDigit dig on dig.id = m.SwDigit
    join Dependencies.Duration_Availability da on da.id = m.DurationAvailability
    join Dependencies.Availability av on av.id = da.AvailabilityId
    join Dependencies.Duration dur on dur.id = da.YearId

    left join Fsp.SwFspCodeTranslation fsp on fsp.SwDigitId = m.SwDigit and fsp.AvailabilityId = av.Id and fsp.DurationId = dur.Id 

    left join [References].Currency cur on cur.id = m.CurrencyReinsurance_Approved

    WHERE   (@sog is null or m.Sog = @sog)
        AND (not exists(select 1 from @digit) or exists(select 1 from @digit where id = m.SwDigit     ))
        AND (not exists(select 1 from @av   ) or exists(select 1 from @av    where id = m.Availability))
        AND (not exists(select 1 from @year ) or exists(select 1 from @year  where id = dur.Id        ))
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'SW-PARAM-OVERVIEW');

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'SOG Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Digit', 'Digit', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SwProduct', 'SW Product Order no.', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'DigitDescription', 'SW Product Order no. description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'G_MATNR', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'FspDescription', 'G_MAKTX', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), '1stLevelSupportCosts', '1st level support costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), '2ndLevelSupportCosts', '2nd level support costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'TotalIb', 'Installed base country', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'IB_SFAB', 'Installed base SFAB', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CurrencyReinsurance', 'Currency Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReinsuranceFlatfee', 'Reinsurance flatfee', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'ShareSwSpMaintenanceListPrice', 'Share Reinsurance of SW/SP Maintenance List Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'RecommendedSwSpMaintenanceListPrice', '(Recommended???) SW/SP Maintenance List Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'DiscountDealerPrice', 'Discount to Dealer price in %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupForProductMarginSwLicenseListPrice', 'Markup for Product Margin of SW License List Price (%)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'LocalRemoteAccessSetupPreparationEffort', 'Local Remote-Access setup preparation effort', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'LocalRegularUpdateReadyEffort', 'Local regular update ready for service effort', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'LocalPreparationShcEffort', 'Local preparation SHC effort', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CentralExecutionShcReportCost', 'Central execution SHC & report cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'LocalRemoteShcCustomerBriefingEffort', 'Local remote SHC customer briefing effort', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'LocalOnSiteShcCustomerBriefingEffort', 'Local on-site SHC customer briefing effort', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'TravellingTime', 'Travelling Time (MTTT)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'OnSiteHourlyRate', 'On-Site Hourly Rate', 1, 1);

set @index = 0;

delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('swdigitsog', 0), 'sog', 'SOG');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('swdigit', 1), 'digit', 'SW digit');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability', 1), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('year', 1), 'year', 'Year');

GO

ALTER PROCEDURE [Report].[spLocapDetailed]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       int,
    @limit        int
)
AS
BEGIN

    declare @sla Portfolio.Sla;

    insert into @sla 
        select   -1
                , Id
                , CountryId
                , WgId
                , AvailabilityId
                , DurationId
                , ReactionTimeId
                , ReactionTypeId
                , ServiceLocationId
                , ProActiveSlaId
                , Sla
                , SlaHash
                , ReactionTime_Avalability
                , ReactionTime_ReactionType
                , ReactionTime_ReactionType_Avalability
                , null
                , null
    from Portfolio.GetBySlaSog(@cnt, (select SogId from InputAtoms.Wg where id = @wg), @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    with cte as (
        select m.* 
        from Hardware.GetCostsSlaSog(1, @sla) m
        where (@wg is null or m.WgId = @wg)
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select     m.Id
             , m.Fsp
             , m.WgDescription
             , m.Wg
             , sog.Description as SogDescription
             , m.ServiceLocation as ServiceLevel
             , m.ReactionTime
             , m.Year as ServicePeriod
             , m.Sog
             , m.ProActiveSla
             , m.Country

             , m.ServiceTcSog * m.ExchangeRate as ServiceTC
             , m.ServiceTpSog * m.ExchangeRate as ServiceTP_Released
             , m.ListPrice * m.ExchangeRate as ListPrice
             , m.DealerPrice * m.ExchangeRate as DealerPrice
             , m.FieldServiceCost * m.ExchangeRate as FieldServiceCost
             , m.ServiceSupportCost * m.ExchangeRate as ServiceSupportCost 
             , m.MaterialOow * m.ExchangeRate as MaterialOow
             , m.MaterialW * m.ExchangeRate as MaterialW
             , m.TaxAndDutiesW * m.ExchangeRate as TaxAndDutiesW
             , m.Logistic * m.ExchangeRate as LogisticW
             , m.Logistic * m.ExchangeRate as LogisticOow
             , m.Reinsurance * m.ExchangeRate as Reinsurance
             , m.Reinsurance * m.ExchangeRate as ReinsuranceOow
             , m.OtherDirect * m.ExchangeRate as OtherDirect
             , m.Credits * m.ExchangeRate as Credits
             , m.LocalServiceStandardWarranty * m.ExchangeRate as LocalServiceStandardWarranty
             , cur.Name as Currency

             , null as IndirectCostOpex
             , m.Availability                       + ', ' +
                   m.ReactionType                   + ', ' +
                   m.ReactionTime                   + ', ' +
                   cast(m.Year as nvarchar(1))      + ', ' +
                   m.ServiceLocation                + ', ' +
                   m.ProActiveSla as ServiceType
         
             , null as PlausiCheck
             , null as PortfolioType

    from cte2 m
    join InputAtoms.Sog sog on sog.id = m.SogId
    join [References].Currency cur on cur.Id = m.CurrencyId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END

go

ALTER PROCEDURE [Report].[spLocap]
(
    @cnt          bigint,
    @wg           bigint,
    @av           bigint,
    @dur          bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       int,
    @limit        int
)
AS
BEGIN

    declare @sla Portfolio.Sla;

    insert into @sla 
        select   -1
                , Id
                , CountryId
                , WgId
                , AvailabilityId
                , DurationId
                , ReactionTimeId
                , ReactionTypeId
                , ServiceLocationId
                , ProActiveSlaId
                , Sla
                , SlaHash
                , ReactionTime_Avalability
                , ReactionTime_ReactionType
                , ReactionTime_ReactionType_Avalability
                , null
                , null
    from Portfolio.GetBySlaSog(@cnt, (select SogId from InputAtoms.Wg where id = @wg), @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    with cte as (
        select m.* 
        from Hardware.GetCostsSlaSog(1, @sla) m
        where (@wg is null or m.WgId = @wg)
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    m.Id
            , m.Fsp
            , m.WgDescription
            , m.ServiceLevel

            , m.ReactionTime
            , m.Year as ServicePeriod
            , m.Wg

            , m.LocalServiceStandardWarranty * m.ExchangeRate as LocalServiceStandardWarranty
            , m.ServiceTcSog * m.ExchangeRate as ServiceTC
            , m.ServiceTpSog  * m.ExchangeRate as ServiceTP_Released
            , cur.Name as Currency
         
            , m.Country
            , m.Availability                       + ', ' +
                  m.ReactionType                   + ', ' +
                  m.ReactionTime                   + ', ' +
                  cast(m.Year as nvarchar(1))      + ', ' +
                  m.ServiceLocation                + ', ' +
                  m.ProActiveSla as ServiceType

            , null as PlausiCheck
            , null as PortfolioType
            , null as ReleaseCreated
            , m.Sog

    from cte2 m
    join [References].Currency cur on cur.Id = m.CurrencyId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);

END
go

ALTER FUNCTION [Report].[SwServicePriceList]
(
    @sog bigint,
    @digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
      LicenseDescription nvarchar(max) NULL
    , Sog nvarchar(max) NULL
    , Fsp nvarchar(max) NULL
    , ServiceDescription nvarchar(max) NULL
    , ServiceShortDescription nvarchar(max) NULL
      
    , TP float NULL
    , DealerPrice float NULL
    , ListPrice float NULL
)
as
begin

    declare @digitList dbo.ListId; 

    if @sog is not null or @digit is not null
    begin

        insert into @digitList(id)
        select Id
        from InputAtoms.SwDigit 
        where     (@sog is null   or SogId = @sog) 
              and (@digit is null or Id = @digit);

        if not exists(select * from @digitList) return;

    end

    declare @avList dbo.ListId; 
    if @av is not null insert into @avList(id) values(@av);

    declare @yearList dbo.ListId; 
    if @year is not null insert into @yearList(id) values(@year);

    insert into @tbl
    select 
              lic.Description as LicenseDescription
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as ServiceDescription
            , fsp.ShortDescription as ServiceShortDescription

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @avList, @yearList, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog

    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                              and fsp.DurationId = sw.Year
                                              and fsp.SwDigitId = sw.SwDigit

    outer apply (

        --get first existing row with valid description

        SELECT top(1) lic.Description
        FROM InputAtoms.SwLicense lic
        WHERE lic.Description IS NOT NULL and exists (select * from InputAtoms.SwDigitLicense sdl where sdl.SwLicenseId = lic.Id and sdl.SwDigitId = dig.Id)
    ) lic;

    return
end

go

ALTER FUNCTION [Report].[SwServicePriceListDetail]
(
    @sog bigint,
    @digit bigint,
    @av bigint,
    @year bigint
)
RETURNS @tbl TABLE (
      LicenseDescription nvarchar(max) NULL
    , License nvarchar(max) NULL
    , Sog nvarchar(max) NULL
    , Fsp nvarchar(max) NULL
    , ServiceDescription nvarchar(max) NULL
    , ServiceShortDescription nvarchar(max) NULL
      
    , ServiceSupport float NULL
    , Reinsurance float NULL
      
    , TP float NULL
    , DealerPrice float NULL
    , ListPrice float NULL
)
as
begin
    declare @digitList dbo.ListId; 

    if @sog is not null or @digit is not null
    begin

        insert into @digitList(id)
        select Id
        from InputAtoms.SwDigit 
        where     (@sog is null   or SogId = @sog) 
              and (@digit is null or Id = @digit);

        if not exists(select * from @digitList) return;

    end

    declare @avList dbo.ListId; 
    if @av is not null insert into @avList(id) values(@av);

    declare @yearList dbo.ListId; 
    if @year is not null insert into @yearList(id) values(@year);

    insert into @tbl
    select    lic.Description as LicenseDescription
            , lic.Name as License
            , sog.Name as Sog
            , fsp.Name as Fsp

            , fsp.ServiceDescription as ServiceDescription
            , fsp.ShortDescription as ServiceShortDescription

            , sw.ServiceSupport as ServiceSupport
            , sw.Reinsurance as Reinsurance

            , sw.TransferPrice as TP
            , sw.DealerPrice as DealerPrice
            , sw.MaintenanceListPrice as ListPrice

    from SoftwareSolution.GetCosts(1, @digitList, @avList, @yearList, -1, -1) sw
    join InputAtoms.SwDigit dig on dig.Id = sw.SwDigit
    join InputAtoms.Sog sog on sog.id = sw.Sog
    left join InputAtoms.SwDigitLicense diglic on dig.Id = diglic.SwDigitId
    left join InputAtoms.SwLicense lic on lic.Id = diglic.SwLicenseId
    left join Fsp.SwFspCodeTranslation fsp on fsp.AvailabilityId = sw.Availability
                                          and fsp.DurationId = sw.Year
                                          and fsp.SwDigitId = sw.SwDigit;

    return;
end

go