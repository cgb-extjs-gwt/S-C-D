using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations._2020_06
{
    public class Migration_2020_06_04_19_19 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 180;

        public string Description => "Upload to SAP ver2";

        public Migration_2020_06_04_19_19(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            //create table [dbo].[SapMappings]
            this.repositorySet.ExecuteSql(@"
            CREATE TABLE [dbo].[SapMappings](
	            [Id] [bigint] IDENTITY(1,1) NOT NULL,
	            [CountryId] [bigint] NOT NULL,
	            [SapCountryName] [nvarchar](255) NOT NULL,
	            [SapCountrySign] [nvarchar](100) NOT NULL,
	            [SapDivision] [nvarchar](100) NULL,	
	            [SapSalesOrganization] [nvarchar](100) NOT NULL,
	            CONSTRAINT [PK_SapMappings] PRIMARY KEY CLUSTERED 
	            (
		            [Id] ASC
	            )
            ) ON [PRIMARY]

            ALTER TABLE [dbo].[SapMappings]  WITH CHECK ADD  CONSTRAINT [FK_SapMappingsCountryId_InputAtomsCountryId] FOREIGN KEY([CountryId])
            REFERENCES [InputAtoms].[Country] ([Id])

            ALTER TABLE [dbo].[SapMappings] CHECK CONSTRAINT [FK_SapMappingsCountryId_InputAtomsCountryId]
            ");

            //create table  [dbo].[SapExportLogs] 
            this.repositorySet.ExecuteSql(@"
            CREATE TABLE [dbo].[SapExportLogs] 
            (
	            [Id] [bigint] IDENTITY(1,1) NOT NULL,
	            [UploadDate] [datetime2](7) NOT NULL,
                [PeriodStartDate] [datetime2](7) NULL,
	            [ExportType] [int] NOT NULL,
                [FileNumber] [int] NOT NULL,
                [IsSend] [bit] NOT NULL,
	            CONSTRAINT [PK_SapExportLogs] PRIMARY KEY CLUSTERED 
	            (
		            [Id] ASC
	            )
            )
            ");

            //create table  [dbo].[SapTables] 
            this.repositorySet.ExecuteSql(@"
            CREATE TABLE [dbo].[SapTables] 
            (
	            [Id] [bigint] IDENTITY(1,1) NOT NULL,
	            [Name] [nvarchar](100) NOT NULL,
	            [SapUploadPackType] [nvarchar](100) NOT NULL,
                [SapSalesOrganization] [nvarchar](100) NOT NULL,
	            CONSTRAINT [PK_SapTables] PRIMARY KEY CLUSTERED 
	            (
		            [Id] ASC
	            )
            )
            ");

            //create procedure [Report].[LocapSapUpload]
            this.repositorySet.ExecuteSql(@"
            IF OBJECT_ID('[Report].[LocapSapUpload]') IS NOT NULL
                DROP PROCEDURE [Report].[LocapSapUpload]; 
            go 


            create PROCEDURE Report.LocapSapUpload
            (
            @periodStartDate DateTime=null
            )
            AS
            BEGIN

            Declare @stepTable Table(Id bigint ,fsp nvarchar(32),WgDesc nvarchar(max), ServiceLevel nvarchar(max),
            Duration nvarchar(500),ServiceLocation nvarchar(max), [Availability] varchar(500), ReactionTime nvarchar(100),
            ReactionType nvarchar(100), ProActiveSla nvarchar(100),  ServicePeriod varchar(500), Wg nvarchar(100),
            PLA nvarchar(max), StdWarranty int, StdWarrantyLocation nvarchar(max), LocalServiceStandardWarranty float, 
            ServiceTC float, ServiceTP_Released float , ReleaseDate datetime, Currency nvarchar(100),
            Country nvarchar(500), ServiceType nvarchar(max), PlausiCheck nvarchar(max), PortfolioType nvarchar(max) , Sog nvarchar(500));

            Declare @resultTable Table([PortfolioId] bigint,Currency nvarchar(100), CountryId int, fsp nvarchar(32), ServiceTP_Released float,
            Wg nvarchar(100), LocalServiceStandardWarranty float, ReleaseDate datetime, NextSapUploadDate datetime,
            SapItemCategory nvarchar(10), SapSalesOrganization nvarchar(100), SapDivision nvarchar(100));
                         
            declare @cntTable dbo.ListId; 
            declare @countryId int;
            declare @wg dbo.ListId; 

            select m.*, lp.Sla, lp.SlaHash, lp.CountryId
            into #shortManualCost
            from [Hardware].[ManualCost] m
            inner join [Portfolio].[LocalPortfolio] lp on m.PortfolioId = lp.Id
            where (@periodStartDate is NULL or m.NextSapUploadDate >= @periodStartDate)
            and m.NextSapUploadDate is not NULL
            and m.SapUploadDate is NULL

            insert into @cntTable(id) 
            select distinct CountryId
            from #shortManualCost;

            declare country_cursor cursor
            for select Id from @cntTable

            open country_cursor;

            FETCH NEXT FROM country_cursor INTO @countryId;

            WHILE @@FETCH_STATUS = 0  
                BEGIN
		            delete from @stepTable

		            insert into @stepTable
		            Exec [Report].[spLocap] @countryId, @wg, null, null, null, null, null, null, null        

		            insert into @resultTable([PortfolioId], Currency, CountryId, fsp, ServiceTP_Released, Wg, LocalServiceStandardWarranty, ReleaseDate,
		                NextSapUploadDate, SapItemCategory, SapSalesOrganization, SapDivision)
		            select distinct s.Id, s.Currency, @countryId as CountryId, s.fsp, s.ServiceTP_Released, s.Wg, s.LocalServiceStandardWarranty, s.ReleaseDate,
		                m.[NextSapUploadDate], fsp.[SapItemCategory], sm.[SapSalesOrganization], sm.[SapDivision]
		            from @stepTable s
		            inner join #shortManualCost m on s.Id = m.PortfolioId		
		            --inner join [Fsp].[HwFspCodeTranslation] fsp on m.Sla = fsp.Sla and m.SlaHash = fsp.SlaHash
		            inner join [Fsp].[HwFspCodeTranslation] fsp on s.fsp = fsp.[Name]
		            inner join [dbo].[SapMappings] sm on sm.[CountryId] = @countryId

		            FETCH NEXT FROM country_cursor INTO @countryId;
                END;

            CLOSE country_cursor;  
            DEALLOCATE country_cursor; 
            drop table #shortManualCost;

            select distinct [PortfolioId], Currency, CountryId, fsp as FspCode, ServiceTP_Released as ServiceTP, Wg as WgName, LocalServiceStandardWarranty as LocalServiceStdw, ReleaseDate,
		                NextSapUploadDate, SapItemCategory, SapSalesOrganization, SapDivision
            from @resultTable

            End

            ");

            this.repositorySet.ExecuteSql(@"ALTER TABLE [Hardware].[ManualCost] ADD [SapUploadDate] DATETIME NULL");

            this.repositorySet.ExecuteSql(@"ALTER TABLE [Hardware].[ManualCost] ADD [NextSapUploadDate] DATETIME NULL");

            this.repositorySet.ExecuteSql(@"ALTER TABLE [Fsp].[HwFspCodeTranslation] ADD [SapItemCategory] [nvarchar](10) NULL");

            this.repositorySet.ExecuteSql(@"ALTER TABLE [Temp].[HwFspCodeTranslation] ADD [SapItemCategory] [nvarchar](10) NULL");

            //update procedure [Temp].[CopyHwFspCodeTranslations]
            this.repositorySet.ExecuteSql(@"
            alter PROCEDURE [Temp].[CopyHwFspCodeTranslations]
            AS
            BEGIN

	            BEGIN TRY
		            BEGIN TRANSACTION


		            TRUNCATE TABLE [Fsp].[HwFspCodeTranslation];

		            INSERT INTO [Fsp].[HwFspCodeTranslation]
				               ([AvailabilityId]
					               ,[CountryId]
					               ,[CreatedDateTime]
					               ,[DurationId]
					               ,[EKKey]
					               ,[EKSAPKey]
					               ,[Name]
					               ,[ProactiveSlaId]
					               ,[ReactionTimeId]
					               ,[ReactionTypeId]
					               ,[SCD_ServiceType]
					               ,[SecondSLA]
					               ,[ServiceDescription]
					               ,[ServiceLocationId]
					               ,[ServiceType]
					               ,[Status]
					               ,[WgId]
					               ,[IsStandardWarranty]
					               ,[LUT]
					               ,[SapItemCategory])
		            SELECT      [AvailabilityId]
				               ,[CountryId]
				               ,[CreatedDateTime]
				               ,[DurationId]
				               ,[EKKey]
				               ,[EKSAPKey]
				               ,[Name]
				               ,[ProactiveSlaId]
				               ,[ReactionTimeId]
				               ,[ReactionTypeId]
				               ,[SCD_ServiceType]
				               ,[SecondSLA]
				               ,[ServiceDescription]
				               ,[ServiceLocationId]
				               ,[ServiceType]
				               ,[Status]
				               ,[WgId]
				               ,[IsStandardWarranty]
				               ,[LUT]
				               ,[SapItemCategory]
		             FROM [Temp].[HwFspCodeTranslation];

                    exec Fsp.spUpdateHwStandardWarranty;

		            COMMIT
	            END TRY

	            BEGIN CATCH
		            IF @@TRANCOUNT > 0
			            ROLLBACK
	            END CATCH

            end
            ");
        }
    }
}
