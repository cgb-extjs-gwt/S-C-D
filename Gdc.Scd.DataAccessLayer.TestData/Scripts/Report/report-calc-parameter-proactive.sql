IF OBJECT_ID('Report.CalcParameterProActive') IS NOT NULL
  DROP FUNCTION Report.CalcParameterProActive;
go 

CREATE FUNCTION Report.CalcParameterProActive
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
         , 'EUR' as Currency
         , pro.LocalRemoteAccessSetupPreparationEffort_Approved as LocalRemoteSetup
         , pro.LocalRemoteShcCustomerBriefingEffort_Approved as LocalRemoteShc
         , pro.LocalOnSiteShcCustomerBriefingEffort_Approved as LocalOnsiteShc
         , pro.LocalRegularUpdateReadyEffort_Approved as LocalRegularUpdate
         , pro.LocalPreparationShcEffort_Approved as LocalPreparationShc
         , pro.TravellingTime_Approved as TravellingTime
         , pro.OnSiteHourlyRate_Approved as OnSiteHourlyRate
         , null as CentralSetup
         , pro.CentralExecutionShcReportCost_Approved as CentralShc

         , wg.Sog

    from Hardware.ProActive pro
    join InputAtoms.Country c on c.id = pro.Country
    join InputAtoms.WgSogView wg on wg.Id = pro.Wg

    where pro.Country = @cnt
      and (@wg is null or pro.Wg = @wg)
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCULATION-PARAMETER-PROACTIVE');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalRemoteSetup', 'Local Remote-Access setup preparation', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalRemoteShc', 'Local remote SHC customer briefing', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalOnsiteShc', 'Local onsite SHC customer briefing', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalRegularUpdate', 'Local regular update ready for service', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'LocalPreparationShc', 'Local preparation SHC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 2, 'TravellingTime', 'Travelling Time (MTTT)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'OnSiteHourlyRate', 'Onsite hourly rate', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'CentralSetup', 'Setup Central Remote-Access & AutoCall', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'CentralShc', 'Central execution SHC & report', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');

GO



