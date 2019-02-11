USE [Scd_2]
GO

DELETE FROM [dbo].[RolePermission] WHERE PermissionId=1 and RoleId=5
GO

ALTER TABLE [InputAtoms].[RoleCode]
     ADD Deactivated bit
GO