IF OBJECT_ID('Report.HddRetentionParameter') IS NOT NULL
  DROP FUNCTION Report.HddRetentionParameter;
go 

CREATE FUNCTION Report.HddRetentionParameter
(
    @wg bigint,
    @year bigint
)
RETURNS TABLE 
AS
RETURN (
select 
       wg.Name as Wg
     , wg.Description as WgDescription
     , y.Name as Year
     , hdd.HddFr_Approved as HddFr
     , hdd.HddMaterialCost_Approved as HddMaterialCost

from Hardware.HddRetention hdd
join InputAtoms.WgSogView wg on wg.Id = hdd.Wg
join Dependencies.Year y on y.Id = hdd.Year

where (@wg is null or wg.Id = @wg)
  and (@year is null or hdd.Year = @year)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-PARAMETER');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Year', 'Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'HddFr', 'FR(year)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'HddMaterialCost', 'MC(Year)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'year', 'Year');

GO