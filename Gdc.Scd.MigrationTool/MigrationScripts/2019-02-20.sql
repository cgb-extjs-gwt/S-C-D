USE [Scd_2]
GO

INSERT INTO [dbo].[RolePermission]
           ([PermissionId]
           ,[RoleId])
     VALUES
           ((select id from dbo.Permission where Name='CostEditor')
           ,(select id from dbo.Role where Name='PRS PSM'))
GO


