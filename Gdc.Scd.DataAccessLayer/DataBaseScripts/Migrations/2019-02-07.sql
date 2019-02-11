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



