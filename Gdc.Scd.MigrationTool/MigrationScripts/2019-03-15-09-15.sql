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
            , wg.Sog as Sog
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
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'TransferPrice', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg', 0), 'wg', 'Warranty Group');
GO

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
    with HDDC as (
        select * 
        from Fsp.HwHddFspCodeTranslation fsp
        where fsp.EKKey = 'HDDC'AND (@cnt is null or fsp.CountryId = @cnt)
    )
    , HDDT as (
        select * 
        from Fsp.HwHddFspCodeTranslation fsp
        where fsp.EKKey = 'HDDT' AND (@cnt is null or fsp.CountryId = @cnt)
    )
    , HddFsp as (
        select coalesce(c.CountryId, t.CountryId) as CountryId 
             , coalesce(c.WgId, t.WgId) as WgId
             , c.Name AS C_Fsp
             , t.Name as T_Fsp
        from HDDC c
        full join HDDT t on t.WgId = c.WgId and t.CountryId = c.CountryId
    )
    SELECT    c.CountryGroup
            , c.Name as Country
            , wg.Name as Wg
            , wg.Description as WgDescription
            , wg.Sog as Sog
            , fsp.C_Fsp as Fsp
            , fsp.T_Fsp as TopFsp

            , hdd.TransferPrice * er.Value as TransferPrice
            , hdd.DealerPrice * er.Value as DealerPrice
            , hdd.ListPrice * er.Value as ListPrice
            , c.Currency

    from HddFsp fsp
    join InputAtoms.CountryView c on c.Id = fsp.CountryId
    join InputAtoms.WgSogView wg on wg.id = fsp.WgId
    left join Hardware.HddRetentionView hdd on hdd.WgId = fsp.WgId
    join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where     (@cnt is null or fsp.CountryId = @cnt)
          and (@wg is null or fsp.WgId = @wg)
)

go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-COUNTRY');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Support Pack Code', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'TopFsp', 'TopUp Code', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TransferPrice', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('usercountry', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg', 0), 'wg', 'Warranty Group');

go
