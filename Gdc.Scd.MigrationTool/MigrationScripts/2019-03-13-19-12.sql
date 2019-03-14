DROP VIEW [Hardware].[HddRetentionView]
GO

CREATE VIEW [Hardware].[HddRetentionView] as 
    SELECT 
           h.Wg as WgId
         , wg.Name as Wg
		 , sog.Name as Sog
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
	LEFT JOIN InputAtoms.Sog sog on sog.id = wg.SogId
    LEFT JOIN Hardware.HddRetentionManualCost hm on hm.WgId = h.Wg
    LEFT JOIN [dbo].[User] u on u.Id = hm.ChangeUserId
    WHERE h.DeactivatedDateTime is null 
      AND h.Year = (select id from Dependencies.Year where Value = 5 and IsProlongation = 0)
GO


