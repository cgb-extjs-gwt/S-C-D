INSERT INTO [dbo].[RolePermission]
           ([PermissionId]
           ,[RoleId])
     VALUES
           ((select Id from dbo.Permission where Name='CostEditor')
           ,(select Id from dbo.Role where Name='GTS User'))
GO

INSERT INTO [dbo].[RolePermission]
           ([PermissionId]
           ,[RoleId])
     VALUES
           ((select Id from dbo.Permission where Name='OwnApproval')
           ,(select Id from dbo.Role where Name='GTS User'))
GO

DELETE FROM [dbo].[RolePermission]
WHERE [PermissionId]=(select top 1 Id from dbo.Permission where Name='Approval')
        and [RoleId]=(select top 1 Id from dbo.Role where Name='PRS PSM')
GO