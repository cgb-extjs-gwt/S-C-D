IF OBJECT_ID('Hardware.HddRetentionManualCost', 'U') IS NOT NULL
  DROP TABLE Hardware.HddRetentionManualCost;
go

CREATE TABLE Hardware.HddRetentionManualCost (
       [WgId] [bigint] NOT NULL primary key foreign key references InputAtoms.Wg(Id)
     , [ChangeUserId] [bigint] NOT NULL foreign key REFERENCES [dbo].[User] ([Id])
     , [TransferPrice] [float] NULL
     , [ListPrice] [float] NULL
     , [DealerDiscount] [float] NULL
     , [DealerPrice]  AS ([ListPrice]-([ListPrice]*[DealerDiscount])/(100))
) 

GO

IF OBJECT_ID('Hardware.HddRetentionView', 'V') IS NOT NULL
  DROP VIEW Hardware.HddRetentionView;
go

CREATE VIEW Hardware.HddRetentionView as 
    SELECT 
           h.Wg as WgId
         , wg.Name as Wg
         , h.HddRet
         , HddRet_Approved
         , hm.TransferPrice 
         , hm.ListPrice
         , hm.DealerDiscount
         , hm.DealerPrice
         , u.Name as ChangeUserName
         , u.Email as ChangeUserEmail

    FROM Hardware.HddRetention h
    JOIN InputAtoms.Wg wg on wg.id = h.Wg
    LEFT JOIN Hardware.HddRetentionManualCost hm on hm.WgId = h.Wg
    LEFT JOIN [dbo].[User] u on u.Id = hm.ChangeUserId
    WHERE h.DeactivatedDateTime is null 
      AND h.Year = (select id from Dependencies.Year where Value = 5 and IsProlongation = 0)
go

insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) 
 values('HDD-RETENTION-CALC-RESULT', 'Hdd retention service costs', 0, 0, 'Report.HddRetentionCalcResult');

 IF OBJECT_ID('Report.HddRetentionCalcResult') IS NOT NULL
  DROP FUNCTION Report.HddRetentionCalcResult;
go 

CREATE FUNCTION Report.HddRetentionCalcResult()
RETURNS TABLE 
AS
RETURN (
    select h.Wg
         , h.TransferPrice
         , h.ListPrice
         , h.DealerDiscount
         , h.DealerPrice
         , h.ChangeUserName + '[' + h.ChangeUserEmail + ']' as ChangeUser
    from Hardware.HddRetentionView h
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-CALC-RESULT');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG(Asset)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TransferPrice', 'Transfer price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'DealerDiscount', 'Dealer discount in %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer price', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ChangeUser', 'Change user', 1, 1);


GO

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
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where UPPER(name) = 'WG'), 'wg', 'Warranty Group');

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
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where UPPER(name) = 'USERCOUNTRY'), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where UPPER(name) = 'WG'), 'wg', 'Warranty Group');




