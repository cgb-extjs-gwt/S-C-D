IF OBJECT_ID('Report.FlatFeeReport') IS NOT NULL
  DROP FUNCTION Report.FlatFeeReport;
go 

CREATE FUNCTION Report.FlatFeeReport
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    select    c.Name as Country
            , c.CountryGroup
            , wg.Name as Wg
            , fee.Fee_Approved as Fee

    from Hardware.AvailabilityFeeCalc fee
    join InputAtoms.CountryView c on c.Id = fee.Country
    join InputAtoms.WgView wg on wg.id = fee.Wg

    where (@cnt is null or fee.Country = @cnt)
      and (@wg is null or fee.Wg = @wg)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'FLAT-FEE-REPORTS');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'Fee', 'FSL Flatfee monthly (EUR)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');

GO