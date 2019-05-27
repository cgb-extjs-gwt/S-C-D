USE [SCD_2]
go

ALTER TABLE [InputAtoms].[Country]
ADD InstallbaseGroup nvarchar(300) null
go
  
UPDATE [InputAtoms].[Country]
SET InstallbaseGroup = 'CEU'
WHERE [Name] IN ('Austria', 'Germany', 'Switzerland', 'Liechtenstein')
go
  
ALTER TABLE [Hardware].[InstallBase]
ADD [InstalledBase1stLevel] float null
go

UPDATE [InputAtoms].[CountryGroup]
SET AutoUploadInstallBase = 1
WHERE [Name] IN ('Austria', 'Germany', 'Suisse')
go