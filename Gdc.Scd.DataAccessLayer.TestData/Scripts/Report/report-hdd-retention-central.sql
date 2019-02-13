IF OBJECT_ID('Report.HddRetentionCentral') IS NOT NULL
  DROP FUNCTION Report.HddRetentionCentral;
go 

CREATE FUNCTION Report.HddRetentionCentral
(
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    select    wg.Name as Wg
            , wg.Description as WgDescription
            , hdd.TransferPrice
            , hdd.DealerPrice as DealerPrice
            , hdd.ListPrice as ListPrice
    from Hardware.HddRetentionView hdd
    join InputAtoms.WgSogView wg on wg.Id = hdd.WgId
    where (@wg is null or wg.Id = @wg)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-CENTRAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TransferPrice', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select top(1) id from Report.ReportFilterType where UPPER(name) = 'WG' and MultiSelect = 0), 'wg', 'Warranty Group');
GO