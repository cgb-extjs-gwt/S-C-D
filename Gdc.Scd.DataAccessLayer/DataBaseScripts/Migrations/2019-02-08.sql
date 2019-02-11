USE [Scd_2]
GO

delete from [dbo].[RolePermission] where PermissionId=1 and RoleId=5
GO

ALTER TABLE [InputAtoms].[RoleCode]
     ADD Deactivated bit
go