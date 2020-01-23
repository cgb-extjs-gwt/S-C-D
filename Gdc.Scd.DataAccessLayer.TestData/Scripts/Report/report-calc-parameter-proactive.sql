IF OBJECT_ID('Report.CalcParameterProActive') IS NOT NULL
  DROP FUNCTION Report.CalcParameterProActive;
go 

CREATE FUNCTION [Report].[CalcParameterProActive]
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN 
(
    select c.Name AS Country
         , wg.Description as WgDescription
         , wg.Name as Wg
         , wg.SCD_ServiceType as ServiceType
         , cur.Name as Currency
         , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteSetup
         , pro.LocalRemoteShcCustomerBriefingEffort_Approved as LocalRemoteShc
         , pro.LocalOnSiteShcCustomerBriefingEffort_Approved as LocalOnsiteShc
         , pro.LocalRegularUpdateReadyEffort_Approved as LocalRegularUpdate
         , pro.LocalPreparationShcEffort_Approved as LocalPreparationShc
         , pro.TravellingTime_Approved as TravellingTime
         , pro.OnSiteHourlyRate_Approved * er.Value as OnSiteHourlyRate
         , pro.CentralExecutionShcReportCost_Approved * er.Value as CentralShc

         , wg.Sog

    from Hardware.ProActive pro
    join InputAtoms.Country c on c.id = pro.Country
    join InputAtoms.WgSogView wg on wg.Id = pro.Wg
	join [References].Currency cur on cur.Id = c.CurrencyId
	join [References].ExchangeRate er on er.CurrencyId = cur.Id

    where pro.Deactivated = 0
	and pro.Country = @cnt
      and (@wg is null or pro.Wg = @wg)
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCULATION-PARAMETER-PROACTIVE');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'LocalRemoteSetup', 'Local Remote-Access setup preparation', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'LocalRemoteShc', 'Local remote SHC customer briefing', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'LocalOnsiteShc', 'Local onsite SHC customer briefing', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'LocalRegularUpdate', 'Local regular update ready for service', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'LocalPreparationShc', 'Local preparation SHC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'TravellingTime', 'Travelling Time (MTTT)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'OnSiteHourlyRate', 'Onsite hourly rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'CentralShc', 'Central execution SHC & report', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg', 0), 'wg', 'Warranty Group');

GO



