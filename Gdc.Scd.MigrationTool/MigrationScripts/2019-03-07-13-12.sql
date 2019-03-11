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
