IF OBJECT_ID('Report.Afr') IS NOT NULL
  DROP FUNCTION Report.Afr;
go 

CREATE FUNCTION Report.Afr
(
    @wg dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select  wg.Name as Wg
          , wg.Description as WgDescription
          , pla.Name as Pla
          , afr.AFR1_Approved * 100 as AFR1
          , afr.AFR2_Approved * 100 as AFR2
          , afr.AFR3_Approved * 100 as AFR3
          , afr.AFR4_Approved * 100 as AFR4
          , afr.AFR5_Approved * 100 as AFR5
    from Hardware.AfrYear afr
    join InputAtoms.Wg wg on wg.Id = afr.Wg and wg.DeactivatedDateTime is null
    left join InputAtoms.Pla pla on pla.Id = wg.PlaId

    where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = afr.Wg))
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'AFR');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Pla', 'Warranty PLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR1', 'Failure Rate 1st year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR2', 'Failure Rate 2nd year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR3', 'Failure Rate 3rd year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR4', 'Failure Rate 4th year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR5', 'Failure Rate 5th year', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgall', 1), 'wg', 'Warranty Group');

GO