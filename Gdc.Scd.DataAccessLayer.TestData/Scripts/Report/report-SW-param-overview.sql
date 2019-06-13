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
        SELECT top(1)
                  ssc.[1stLevelSupportCostsCountry_Approved] / er.Value as [1stLevelSupportCosts]
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
    , cte as (
        select   c.Name as Country
               , sog.Description as SogDescription
               , sog.Name as Sog
               , dig.Name as Digit
               , l.Name as SwProduct
               , dig.Description as DigitDescription

               , av.Name as Availability
               , case when dur.IsProlongation = 0 then CAST(dur.Value as nvarchar(16)) else 'prolongation' end as Duration

               , fsp.Name as Fsp
               , fsp.ShortDescription FspDescription

               , (select [1stLevelSupportCosts] from GermanyServiceCte) as [1stLevelSupportCosts]
               , m.[2ndLevelSupportCosts_Approved] as [2ndLevelSupportCosts]
       
               , (select TotalIb from GermanyServiceCte) as TotalIb
               , m.InstalledBaseSog as IbDigit

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

        join InputAtoms.Country c on c.id = pro.Country
        join InputAtoms.Sog sog on sog.Id = m.Sog
        join InputAtoms.SwDigit dig on dig.id = m.SwDigit
        join Dependencies.Duration_Availability da on da.id = m.DurationAvailability
        join Dependencies.Availability av on av.id = da.AvailabilityId
        join Dependencies.Duration dur on dur.id = da.YearId

        left join Fsp.SwFspCodeTranslation fsp on fsp.SwDigitId = m.SwDigit and fsp.AvailabilityId = av.Id and fsp.DurationId = dur.Id 

        left join [References].Currency cur on cur.id = m.CurrencyReinsurance_Approved
        left join InputAtoms.SwLicense l on fsp.SwLicenseId = l.Id

        WHERE   (@sog is null or m.Sog = @sog)
            AND (not exists(select 1 from @digit) or exists(select 1 from @digit where id = m.SwDigit     ))
            AND (not exists(select 1 from @av   ) or exists(select 1 from @av    where id = m.Availability))
            AND (not exists(select 1 from @year ) or exists(select 1 from @year  where id = dur.Id        ))
            AND m.DeactivatedDateTime is null
    )
    select *
           , case when TotalIb > 0 then [1stLevelSupportCosts] / TotalIb end as [1stLevelSupportCosts_Calc]
           , case when IbDigit > 0 then [2ndLevelSupportCosts] / IbDigit end as [2ndLevelSupportCosts_Calc]
    from cte
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
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'IbDigit', 'Installed base', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), '1stLevelSupportCosts_Calc', 'Calculated 1st level support costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), '2ndLevelSupportCosts_Calc', 'Calculated 2nd level support costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CurrencyReinsurance', 'Currency Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReinsuranceFlatfee', 'Reinsurance flatfee', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'ShareSwSpMaintenanceListPrice', 'Share Reinsurance of SW/SP Maintenance List Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'RecommendedSwSpMaintenanceListPrice', 'SW/SP Maintenance List Price', 1, 1);
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