use Scd_2_hdd;

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
         , u.Name as ChangeUser
         , u.Email as ChangeUserEmail

    FROM Hardware.HddRetention h
    JOIN InputAtoms.Wg wg on wg.id = h.Wg
    LEFT JOIN Hardware.HddRetentionManualCost hm on hm.WgId = h.Wg
    LEFT JOIN [dbo].[User] u on u.Id = hm.ChangeUserId
    WHERE h.DeactivatedDateTime is null 
      AND h.Year = (select id from Dependencies.Year where Value = 5 and IsProlongation = 0)
go
