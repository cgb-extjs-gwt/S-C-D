USE [Scd_2]
GO

INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (113,'/02/sites/p/ServiceCostDatabase/CGER/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CGER')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (80,'/02/sites/p/ServiceCostDatabase/CSWE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CSWE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (144,'/02/sites/p/ServiceCostDatabase/CSWE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CSWE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (97,'/02/sites/p/ServiceCostDatabase/UKandI/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/UKandI')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (121,'/02/sites/p/ServiceCostDatabase/CSWE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CSWE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (142,'/02/sites/p/ServiceCostDatabase/CSWE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CSWE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (120,'/teams/cor/SCD/USA/CD_CS_CalculationTool','https://partners.ts.fujitsu.com/teams/cor/SCD/USA')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (13,'/02/sites/p/ServiceCostDatabase/CSWE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CSWE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (41,'/02/sites/p/ServiceCostDatabase/CNEE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CNEE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (158,'/02/sites/p/ServiceCostDatabase/CSWE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CSWE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (112,'/02/sites/p/ServiceCostDatabase/CGER/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CGER')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (115,'/02/sites/p/ServiceCostDatabase/CGER/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CGER')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (135,'/02/sites/p/ServiceCostDatabase/CMEA/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CMEA')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (12,'/02/sites/p/ServiceCostDatabase/CNEE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CNEE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (101,'/02/sites/p/ServiceCostDatabase/CNOE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CNOE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (82,'/02/sites/p/ServiceCostDatabase/CSWE/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CSWE')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (118,'/teams/cor/SCD/Latin%20America/CD_CS_CalculationTool','https://partners.ts.fujitsu.com/teams/cor/SCD/Latin%20America')
INSERT INTO [Report].[CdCsConfiguration] ([CountryId],[FileFolderUrl], [FileWebUrl]) VALUES (156,'/02/sites/p/ServiceCostDatabase/CMEA/CD_CS_CalculationTool','http://emeia.fujitsu.local/02/sites/p/ServiceCostDatabase/CMEA')
GO

ALTER FUNCTION [Report].[GetServiceCostsBySla]
(
    @cnt nvarchar(200),
    @loc nvarchar(200),
    @av nvarchar(200),
    @reactiontime nvarchar(200),
    @reactiontype nvarchar(200),
    @wg nvarchar(200),   
    @dur nvarchar(200)
)
RETURNS @tbl TABLE (
    Country nvarchar(200),
    ServiceTC float, 
    ServiceTP float, 
    ServiceTP1 float,
    ServiceTP2 float,
    ServiceTP3 float,
    ServiceTP4 float,
    ServiceTP5 float
)
AS
BEGIN

    declare @cntId dbo.ListId;
    declare @locId dbo.ListId;
    declare @avId dbo.ListId;
    declare @reactiontimeId dbo.ListId;
    declare @reactiontypeId dbo.ListId;
    declare @wgId dbo.ListId;
    declare @durId dbo.ListId;
	declare @proId dbo.ListId;

    insert into @cntId select id from InputAtoms.Country where UPPER(Name)= UPPER(@cnt);
    insert into @locId select  id from Dependencies.ServiceLocation where UPPER(Name) = UPPER(@loc);
    insert into @avId select   id from Dependencies.Availability where ExternalName like '%' + @av + '%';
    insert into @reactiontimeId select   id from Dependencies.ReactionTime where UPPER(Name)=UPPER(@reactiontime);
    insert into @reactiontypeId select   id from Dependencies.ReactionType where UPPER(Name)=UPPER(@reactiontype);
    insert into @wgId select id from InputAtoms.Wg where UPPER(Name)=UPPER(@wg);
    insert into @durId select id from Dependencies.Duration where UPPER(Name)=UPPER(@dur);

    INSERT @tbl
    select costs.Country,
           coalesce(costs.ServiceTCManual, costs.ServiceTC) as ServiceTC, 
		   costs.ServiceTP_Released as ServiceTP, 
           costs.ServiceTP1,
           costs.ServiceTP2,
           costs.ServiceTP3,
           costs.ServiceTP4,
           costs.ServiceTP5
     from Hardware.GetCostsFull(1, @cntId, @wgId, @avId, @durId, @reactiontimeId, @reactiontypeId, @locId, @proId, 0, -1) costs

    return;

END