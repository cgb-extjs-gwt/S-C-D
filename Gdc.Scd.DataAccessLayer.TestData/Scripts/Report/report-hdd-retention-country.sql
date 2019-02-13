IF OBJECT_ID('Report.HddRetentionByCountry') IS NOT NULL
  DROP FUNCTION Report.HddRetentionByCountry;
go 

CREATE FUNCTION Report.HddRetentionByCountry
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    SELECT c.CountryGroup
         , c.Name as Country
         , wg.Name as Wg
         , wg.Description as WgDescription
         , fsp.Name as Fsp
         , fsp.Name as TopFsp

         , hdd.TransferPrice
         , hdd.DealerPrice
         , hdd.ListPrice

    from Fsp.HwHddFspCodeTranslation fsp
    join InputAtoms.CountryView c on c.Id = fsp.CountryId
    join InputAtoms.WgSogView wg on wg.id = fsp.WgId
    left join Hardware.HddRetentionView hdd on hdd.WgId = fsp.WgId

    where     (@cnt is null or fsp.CountryId = @cnt)
          and (@wg is null or fsp.WgId = @wg)
)

go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-COUNTRY');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Support Pack Code', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TopFsp', 'TopUp Code', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TransferPrice', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select top(1) id from Report.ReportFilterType where UPPER(name) = 'USERCOUNTRY' and MultiSelect = 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select top(1) id from Report.ReportFilterType where UPPER(name) = 'WG' and MultiSelect = 0), 'wg', 'Warranty Group');


