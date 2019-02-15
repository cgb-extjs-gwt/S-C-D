ALTER TABLE [InputAtoms].[RoleCode] 
 ADD [CreatedDateTime] [datetime2](7) NOT NULL DEFAULT (getutcdate())
GO
ALTER TABLE [InputAtoms].[RoleCode] 
 ADD [ModifiedDateTime] [datetime2](7) NOT NULL DEFAULT (getutcdate())
GO
ALTER TABLE [InputAtoms].[RoleCode] 
 ADD [DeactivatedDateTime] [datetime2](7) NULL
GO
ALTER TABLE [InputAtoms].[RoleCode] 
 DROP CONSTRAINT [DF__RoleCode__Deacti__1C68D709];
GO
ALTER TABLE [InputAtoms].[RoleCode] 
 DROP COLUMN [Deactivated]
GO
