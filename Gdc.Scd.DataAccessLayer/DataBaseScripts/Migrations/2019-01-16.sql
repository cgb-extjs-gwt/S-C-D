USE [Scd_2]

INSERT INTO [dbo].[Permission] (Name) VALUES ('CostImport')

DECLARE @CostImportId BIGINT = SCOPE_IDENTITY()

INSERT INTO [dbo].[RolePermission] (PermissionId, RoleId) 
VALUES (@CostImportId, (SELECT [Id] FROM [dbo].[Role] WHERE [Role].Name = 'PRS PSM'))

INSERT INTO [dbo].[RolePermission] (PermissionId, RoleId) 
VALUES (@CostImportId, (SELECT [Id] FROM [dbo].[Role] WHERE [Role].Name = 'Country key user'))

INSERT INTO [dbo].[RolePermission] (PermissionId, RoleId) 
VALUES (@CostImportId, (SELECT [Id] FROM [dbo].[Role] WHERE [Role].Name = 'PRS Finance'))

INSERT INTO [dbo].[RolePermission] (PermissionId, RoleId) 
VALUES (@CostImportId, (SELECT [Id] FROM [dbo].[Role] WHERE [Role].Name = 'Spares Logistics'))

INSERT INTO [dbo].[RolePermission] (PermissionId, RoleId) 
VALUES (@CostImportId, (SELECT [Id] FROM [dbo].[Role] WHERE [Role].Name = 'GTS user'))