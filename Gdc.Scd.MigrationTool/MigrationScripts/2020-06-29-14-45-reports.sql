IF OBJECT_ID('[ProjectCalculator].[spLocapReport]') IS NOT NULL
  DROP PROCEDURE [ProjectCalculator].[spLocapReport]
GO

CREATE PROCEDURE [ProjectCalculator].[spLocapReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId; 
	IF @cnt IS NOT NULL 
		insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where SogId in (
            select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
        )
        and IsSoftware = 0
        and SogId is not null
        and DeactivatedDateTime is null;

    if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
        where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    m.Id
            , m.Fsp
            , m.WgDescription
            , m.ServiceLevel

            , m.Duration
            , m.ServiceLocation
            , m.Availability
            , m.ReactionTime
            , m.ReactionType
            , m.ProActiveSla

            , m.ServicePeriod

            , m.Wg
            , pla.Name as PLA

            , m.StdWarranty
            , m.StdWarrantyLocation

            , m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
            , m.ServiceTcSog * m.ExchangeRate as ServiceTC
            , m.ServiceTpSog_Released  * m.ExchangeRate as ServiceTP_Released
            , m.ReleaseDate

            , m.Currency
         
            , m.Country
            , m.Availability                       + ', ' +
                  m.ReactionType                   + ', ' +
                  m.ReactionTime                   + ', ' +
                  m.ServicePeriod                  + ', ' +
                  m.ServiceLocation                + ', ' +
                  m.ProActiveSla as ServiceType

            , null as PlausiCheck
            , wg.ServiceTypes as PortfolioType
            , m.Sog

    from cte2 m
    INNER JOIN InputAtoms.Wg wg on wg.id = m.WgId
    INNER JOIN InputAtoms.Pla pla on pla.Id = wg.PlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);
END
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Locap'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 1, @reportName, 'ProjectCalculator.spLocapReport', 'LOCAP reports (for a specific country)')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarranty', 'Standard Warranty Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarrantyLocation', 'Standard Warranty Service Location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LocalServiceStandardWarranty', 'Standard Warranty costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTC', 'Service TC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP_Released', 'Service TP (Released)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('datetime'), 'ReleaseDate', 'Release date', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceType', 'Service type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PlausiCheck', 'Plausi Check', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PortfolioType', 'Portfolio Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'Pro Active SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PLA', 'PLA', 1, 1);


set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgsog', 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
GO

-------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('ProjectCalculator.spLocapApprovedReport') IS NOT NULL
  DROP PROCEDURE [ProjectCalculator].[spLocapApprovedReport];
GO

CREATE PROCEDURE [ProjectCalculator].[spLocapApprovedReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId; 
    IF @cnt IS NOT NULL
        insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where SogId in (
            select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
        )
        and IsSoftware = 0
        and SogId is not null
        and DeactivatedDateTime is null;

    if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
        where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    m.Id
            , m.Fsp
            , m.WgDescription
            , m.ServiceLevel

            , m.Duration
            , m.ServiceLocation
            , m.Availability
            , m.ReactionTime
            , m.ReactionType
            , m.ProActiveSla

            , m.ServicePeriod

            , m.Wg
            , pla.Name as PLA
            , m.StdWarranty
            , m.StdWarrantyLocation

            , m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
            
            , m.ServiceTcSog * m.ExchangeRate as ServiceTC
            , m.ServiceTpSog_Released  * m.ExchangeRate as ServiceTP_Released
            , m.ServiceTpSog * m.ExchangeRate as ServiceTP_Approved
            , m.ReleaseDate

            , m.Currency
         
            , m.Country
            , m.Availability                       + ', ' +
                  m.ReactionType                   + ', ' +
                  m.ReactionTime                   + ', ' +
                  m.ServicePeriod                  + ', ' +
                  m.ServiceLocation                + ', ' +
                  m.ProActiveSla as ServiceType

            , null as PlausiCheck
            , wg.ServiceTypes as PortfolioType
            , m.Sog

    from cte2 m
    INNER JOIN InputAtoms.Wg wg on wg.id = m.WgId
    INNER JOIN InputAtoms.Pla pla on pla.Id = wg.PlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);
END
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Locap-Approved'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 1, @reportName, 'ProjectCalculator.spLocapApprovedReport', 'LOCAP reports(approved)')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarranty', 'Standard Warranty Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarrantyLocation', 'Standard Warranty Service Location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LocalServiceStandardWarranty', 'Standard Warranty costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTC', 'Service TC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP_Released', 'Service TP (Released)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP_Approved', 'Service TP (Approved)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReleaseDate', 'Release date', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceType', 'Service type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PlausiCheck', 'Plausi Check', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PortfolioType', 'Portfolio Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'Pro Active SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PLA', 'PLA', 1, 1);


set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('usercountry', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgsog', 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');

GO

-------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('ProjectCalculator.spLocapDetailedReport') IS NOT NULL
  DROP PROCEDURE [ProjectCalculator].[spLocapDetailedReport];
GO

CREATE PROCEDURE [ProjectCalculator].[spLocapDetailedReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId
	IF @cnt IS NOT NULL
		insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where SogId in (
            select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
        )
        and IsSoftware = 0
        and SogId is not null
        and DeactivatedDateTime is null;

    if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
        where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select     m.Id
             , m.Fsp
             , m.WgDescription
             , m.Wg
             , sog.Description as SogDescription
             , pla.Name as PLA
             , m.ServiceLevel

             , m.ServicePeriod

             , m.Duration
             , m.ServiceLocation
             , m.Availability
             , m.ReactionTime
             , m.ReactionType
             , m.ProActiveSla

             , m.Sog
             , m.Country
             , m.StdWarranty
             , m.StdWarrantyLocation

             , m.ServiceTcSog * m.ExchangeRate as ServiceTC
             , m.ServiceTpSog_Released * m.ExchangeRate as ServiceTP_Released

             , m.ReleaseDate

             , m.FieldServiceCost * m.ExchangeRate as FieldServiceCost
             , m.ServiceSupportCost * m.ExchangeRate as ServiceSupportCost 
             , m.MaterialOow * m.ExchangeRate as MaterialOow
             , m.MaterialW * m.ExchangeRate as MaterialW
             , m.TaxAndDutiesW * m.ExchangeRate as TaxAndDutiesW
             , m.Logistic * m.ExchangeRate as LogisticW
             , m.Logistic * m.ExchangeRate as LogisticOow
             , m.Reinsurance * m.ExchangeRate as Reinsurance
             , m.Reinsurance * m.ExchangeRate as ReinsuranceOow
             , m.OtherDirect * m.ExchangeRate as OtherDirect
             , m.Credits * m.ExchangeRate as Credits
             , m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
             , m.Currency

             , m.Availability                       + ', ' +
                   m.ReactionType                   + ', ' +
                   m.ReactionTime                   + ', ' +
                   m.ServicePeriod                  + ', ' +
                   m.ServiceLocation                + ', ' +
                   m.ProActiveSla as ServiceType

    from cte2 m
    INNER JOIN  InputAtoms.Sog sog on sog.id = m.SogId
    INNER JOIN InputAtoms.Pla pla on pla.Id = sog.PlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);
END
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Locap-Detailed'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 1, @reportName, 'ProjectCalculator.spLocapDetailedReport', 'LOCAP reports detailed')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Service Offering Group (SOG) Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarranty', 'Standard Warranty Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarrantyLocation', 'Standard Warranty Service Location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTC', 'Service TC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP_Released', 'Service TP (Released)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('datetime'), 'ReleaseDate', 'Release date', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost', 'Field Service Cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceSupportCost', 'Service Support Cost Maintenance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialOow', 'Material Cost OOW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialW', 'Material Cost Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TaxAndDutiesW', 'Customs Duty base warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticW', 'Logistics Cost Base Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticOow', 'Logistics Cost OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceOow', 'Reinsurance OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'OtherDirect', 'Other direct cost and contingency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'Credits', 'Credits', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LocalServiceStandardWarranty', 'Standard Warranty costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceType', 'Service type', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'Pro Active SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PLA', 'PLA', 1, 1);


set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgsog', 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
GO

-------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('ProjectCalculator.spLocapDetailedApprovedReport') IS NOT NULL
  DROP PROCEDURE [ProjectCalculator].[spLocapDetailedApprovedReport];
GO

CREATE PROCEDURE [ProjectCalculator].[spLocapDetailedApprovedReport]
(
    @cnt          bigint,
    @wg           dbo.ListID readonly,
    @reactiontype bigint,
    @loc          bigint,
    @lastid       bigint,
    @limit        int,
	@projectId  BIGINT
)
AS
BEGIN
	declare @cntTable dbo.ListId; 
	IF @cnt IS NOT NULL 
		insert into @cntTable(id) values(@cnt);

    declare @wg_SOG_Table dbo.ListId;
    insert into @wg_SOG_Table
    select id
        from InputAtoms.Wg 
        where SogId in (
            select wg.SogId from InputAtoms.Wg wg  where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = wg.Id))
        )
        and IsSoftware = 0
        and SogId is not null
        and DeactivatedDateTime is null;

    if not exists(select id from @wg_SOG_Table) return;

    declare @avTable dbo.ListId;

    declare @durTable dbo.ListId;

    declare @rtimeTable dbo.ListId;

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAggregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
        where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
    )
    , cte2 as (
        select  
                ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                , m.*
                , fsp.Name as Fsp
                , fsp.ServiceDescription as ServiceLevel

        from cte m
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select     m.Id
             , m.Fsp
             , m.WgDescription
             , m.Wg
             , sog.Description as SogDescription
             , m.ServiceLevel

             , m.Duration
             , m.ServiceLocation
             , m.Availability
             , m.ReactionTime
             , m.ReactionType
             , m.ProActiveSla

             , m.ServicePeriod
             , m.Sog             
             , pla.Name as PLA
             
             , m.Country

             , m.StdWarranty
             , m.StdWarrantyLocation

             , m.ServiceTcSog * m.ExchangeRate as ServiceTC
             , m.ServiceTpSog * m.ExchangeRate as ServiceTP_Approved
             , m.ServiceTpSog_Released * m.ExchangeRate as ServiceTP_Released

             , m.ReleaseDate

             , m.FieldServiceCost * m.ExchangeRate as FieldServiceCost
             , m.ServiceSupportCost * m.ExchangeRate as ServiceSupportCost 
             , m.MaterialOow * m.ExchangeRate as MaterialOow
             , m.MaterialW * m.ExchangeRate as MaterialW
             , m.TaxAndDutiesW * m.ExchangeRate as TaxAndDutiesW
             , m.Logistic * m.ExchangeRate as LogisticW
             , m.Logistic * m.ExchangeRate as LogisticOow
             , m.Reinsurance * m.ExchangeRate as Reinsurance
             , m.Reinsurance * m.ExchangeRate as ReinsuranceOow
             , m.OtherDirect * m.ExchangeRate as OtherDirect
             , m.Credits * m.ExchangeRate as Credits
             , m.LocalServiceStandardWarrantyWithRisk * m.ExchangeRate as LocalServiceStandardWarranty
             , m.Currency

             , m.Availability                       + ', ' +
                   m.ReactionType                   + ', ' +
                   m.ReactionTime                   + ', ' +
                   m.ServicePeriod                  + ', ' +
                   m.ServiceLocation                + ', ' +
                   m.ProActiveSla as ServiceType

    from cte2 m
    INNER JOIN  InputAtoms.Sog sog on sog.id = m.SogId
    INNER JOIN InputAtoms.Pla pla on pla.Id = sog.PlaId

    where (@limit is null) or (m.rownum > @lastid and m.rownum <= @lastid + @limit);
END
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Locap-Detailed-Approved'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 1, @reportName, 'ProjectCalculator.spLocapDetailedApprovedReport', 'LOCAP reports detailed(approved)')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;
delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Service Offering Group (SOG) Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarranty', 'Standard Warranty Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'StdWarrantyLocation', 'Standard Warranty Service Location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTC', 'Service TC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP_Released', 'Service TP (Released)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP_Approved', 'Service TP (Approved)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('datetime'), 'ReleaseDate', 'Release date', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost', 'Field Service Cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceSupportCost', 'Service Support Cost Maintenance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialOow', 'Material Cost OOW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialW', 'Material Cost Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TaxAndDutiesW', 'Customs Duty base warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticW', 'Logistics Cost Base Warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticOow', 'Logistics Cost OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceOow', 'Reinsurance OOW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'OtherDirect', 'Other direct cost and contingency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'Credits', 'Credits', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LocalServiceStandardWarranty', 'Standard Warranty costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceType', 'Service type', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'Pro Active SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PLA', 'PLA', 1, 1);


set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('usercountry', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wgsog', 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
GO

-------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('[ProjectCalculator].[spContractReport]') is not null
    drop procedure [ProjectCalculator].[spContractReport]
GO

CREATE PROCEDURE [ProjectCalculator].[spContractReport]
(
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @reactiontype BIGINT,
    @loc          BIGINT,
    @pro          BIGINT,
    @lastid       BIGINT,
    @limit        INT,
	@projectId    BIGINT
)
AS
BEGIN
	declare @avTable dbo.ListId

    declare @durTable dbo.ListId; insert into @durTable(id) select id from Dependencies.Duration where IsProlongation = 0 and Value = 5;

    declare @rtimeTable dbo.ListId

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; if @pro is not null insert into @proTable(id) values(@pro);	

	WITH [Costs] AS
	(
		SELECT
			t.[Id],
			t.[Country],
			t.[Wg],
			Wg.[Description] AS WgDescription,
			t.[ServiceLocation],
			t.[ReactionTime],
			t.[ReactionType],
			t.[Availability],

			t.[ServiceTP],
			t.[StdMonths],
			t.[StdIsProlongation],
			t.[StdMonths] % 12 AS RemainderMonths,

			t.[Currency],
			wg.[ServiceTypes] AS PortfolioType,
			wg.[Sog] AS Sog
		FROM 
			[Hardware].[GetCostsYear](
				1, 
				@cnt, 
				@wg, 
				@avTable, 
				@durTable, 
				@rtimeTable, 
				@rtypeTable, 
				@locTable, 
				@proTable,
				@lastid,
				@limit,
				@projectId) AS t
		INNER JOIN
			[InputAtoms].[WgSogView] AS Wg on Wg.id = t.WgId
	),
	[CostMonths] AS 
	(
		SELECT
			[Costs].*,
			(
				CASE
					WHEN [Costs].[RemainderMonths] = 0 
					THEN [Costs].[ServiceTP] / 12
					ELSE [Costs].[ServiceTP] / [Costs].[RemainderMonths]
				END
			) 
			AS ServiceTPMonthly
		FROM
			[Costs]
	)
	SELECT
		MIN([Id]) AS Id,
		[Country],
		[Wg],
		[WgDescription],
		[ServiceLocation],
		[ReactionTime],
		[ReactionType],
		[Availability],

		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [ServiceTP] END) AS ServiceTP1,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [ServiceTP] END) AS ServiceTP2,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [ServiceTP] END) AS ServiceTP3,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [ServiceTP] END) AS ServiceTP4,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [ServiceTP] END) AS ServiceTP5,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [ServiceTP] END) AS ServiceTP6,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [ServiceTP] END) AS ServiceTP7,

		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly1,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly2,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly3,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly4,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly5,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly6,
		MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [ServiceTPMonthly] END) AS ServiceTPMonthly7,

		[Currency],
		[PortfolioType],
		[Sog]
	FROM
		[CostMonths]
	GROUP BY
		[Country],
		[Wg],
		[WgDescription],
		[ServiceLocation],
		[ReactionTime],
		[ReactionType],
		[Availability],

		[Currency],
		[PortfolioType],
		[Sog]
END
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Contract'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 1, @reportName, 'ProjectCalculator.spContractReport', 'Contract reports')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ProActiveSla', 'ProActive SLA', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP1', 'Service Tranfer Price yearly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP2', 'Service Tranfer Price yearly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP3', 'Service Tranfer Price yearly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP4', 'Service Tranfer Price yearly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTP5', 'Service Tranfer Price yearly - year5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly1', 'Service Tranfer Price monthly - year1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly2', 'Service Tranfer Price monthly - year2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly3', 'Service Tranfer Price monthly - year3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly4', 'Service Tranfer Price monthly - year4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ServiceTPMonthly5', 'Service Tranfer Price monthly - year5', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WarrantyLevel', 'Warranty Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'PortfolioType', 'Portfolio Type', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 1), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg', 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');
GO

-------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('[ProjectCalculator].[GetParameterHw]') IS NOT NULL
    DROP FUNCTION [ProjectCalculator].[GetParameterHw];
GO

CREATE FUNCTION [ProjectCalculator].[GetParameterHw]
(
    @approved	  BIT,
    @cnt		  BIGINT,
    @wg			  BIGINT,
    @reactiontype BIGINT,
    @loc		  BIGINT,
    @pro          BIGINT,
	@projectId	  BIGINT
)
RETURNS @result TABLE 
(	
	[Id] BIGINT,
	[Country] NVARCHAR(MAX),
	[WgDescription] NVARCHAR(MAX),
	[Wg] NVARCHAR(MAX),
	[SogDescription] NVARCHAR(MAX),
	[SCD_ServiceType] NVARCHAR(MAX),
	[ServiceLocation] NVARCHAR(MAX),
	[ReactionTime] NVARCHAR(MAX),
	[ReactionType] NVARCHAR(MAX),
	[Availability] NVARCHAR(MAX),
	[Duration] NVARCHAR(MAX),
	[Currency] NVARCHAR(MAX),

	[LabourCost] FLOAT,
	[TravelCost] FLOAT,
	[PerformanceRate] FLOAT,
	[TravelTime] FLOAT,
	[RepairTime] FLOAT,
	[OnsiteHourlyRate] FLOAT,
	[TimeAndMaterialShare] FLOAT,
	[OohUpliftFactor] FLOAT,
	[AvailabilityFee] FLOAT,
	[TaxAndDutiesW] FLOAT,
	[MarkupOtherCost] FLOAT,
	[MarkupFactorOtherCost] FLOAT,
	[MarkupFactorStandardWarranty] FLOAT,
	[MarkupStandardWarranty] FLOAT,
	[RiskFactorStandardWarranty] FLOAT,
	[RiskStandardWarranty] FLOAT,
	[AFR1] FLOAT,
	[AFR2] FLOAT,
	[AFR3] FLOAT,
	[AFR4] FLOAT,
	[AFR5] FLOAT,
	[AFR6] FLOAT,
	[AFR7] FLOAT,
	[1stLevelSupportCosts] FLOAT,
	[2ndLevelSupportCosts] FLOAT,
	[Sar] FLOAT,
	[ReinsuranceFlatfee1] FLOAT,
	[ReinsuranceFlatfee2] FLOAT,
	[ReinsuranceFlatfee3] FLOAT,
	[ReinsuranceFlatfee4] FLOAT,
	[ReinsuranceFlatfee5] FLOAT,
	[ReinsuranceFlatfee6] FLOAT,
	[ReinsuranceFlatfee7] FLOAT,
	[ReinsuranceUpliftFactor] FLOAT,
	[MaterialCostWarranty] FLOAT,
	[MaterialCostOow] FLOAT,
	[FieldServiceCost1] FLOAT,
	[FieldServiceCost2] FLOAT,
	[FieldServiceCost3] FLOAT,
	[FieldServiceCost4] FLOAT,
	[FieldServiceCost5] FLOAT,
	[FieldServiceCost6] FLOAT,
	[FieldServiceCost7] FLOAT,
	[StandardHandling] FLOAT,
	[HighAvailabilityHandling] FLOAT,
	[StandardDelivery] FLOAT,
	[ExpressDelivery] FLOAT,
	[TaxiCourierDelivery] FLOAT,
	[ReturnDeliveryFactory] FLOAT,
	[LogisticsHandling] FLOAT,
	[LogisticTransportcost] FLOAT,
	[IB_per_Country] FLOAT,
	[IB_per_PLA] FLOAT
)
AS
BEGIN
	DECLARE @cntTable [dbo].[ListID]
	IF @cnt IS NOT NULL
		INSERT INTO @cntTable(id) VALUES (@cnt)

	DECLARE @wgTable [dbo].[ListID]
	IF @wg IS NOT NULL
		INSERT INTO @wgTable(id) VALUES (@wg)

	DECLARE @av [dbo].[ListID]
	DECLARE @duration [dbo].[ListID]
	DECLARE @reactiontime [dbo].[ListID]

	DECLARE @reactiontypeTable [dbo].[ListID]
	IF @reactiontype IS NOT NULL
		INSERT INTO @reactiontypeTable(id) VALUES (@reactiontype)
	
	DECLARE @locTable [dbo].[ListID]
	IF @loc IS NOT NULL
		INSERT INTO @locTable(id) VALUES (@loc)

	DECLARE @proTable [dbo].[ListID]
	IF @pro IS NOT NULL
		INSERT INTO @proTable(id) VALUES (@pro);

	WITH [Costs] AS 
	(
		SELECT
			MIN(t.[Id]) AS Id,
			t.[Country],
			t.[CountryId],
			t.[WgId],
			t.[ServiceLocation],
			t.[ReactionTime],
			t.[ReactionType],
			t.[Availability],

			t.[LabourCost],
			t.[TravelCost],
			t.[PerformanceRate],
			t.[TravelTime],
			t.[RepairTime],
			t.[OnsiteHourlyRates],
			t.[TimeAndMaterialShare_norm] * 100 AS TimeAndMaterialShare,
			t.[OohUpliftFactor],
			t.[AvailabilityFee],
			t.[TaxAndDutiesW],
			t.[MarkupOtherCost],
			t.[MarkupFactorOtherCost],
			t.[MarkupFactorStandardWarranty],
			t.[MarkupStandardWarranty],
			t.[RiskFactorStandardWarranty],
			t.[RiskStandardWarranty],

			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [AFR] END) AS AFR1,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [AFR] END) AS AFR2,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [AFR] END) AS AFR3,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [AFR] END) AS AFR4,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [AFR] END) AS AFR5,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [AFR] END) AS AFR6,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [AFR] END) AS AFR7,

			t.[1stLevelSupportCosts],
			t.[2ndLevelSupportCosts],
			t.[Sar],

			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 12)) THEN [AFR] END) AS ReinsuranceFlatfee1,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 24)) THEN [AFR] END) AS ReinsuranceFlatfee2,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 36)) THEN [AFR] END) AS ReinsuranceFlatfee3,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 48)) THEN [AFR] END) AS ReinsuranceFlatfee4,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 60)) THEN [AFR] END) AS ReinsuranceFlatfee5,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 72)) THEN [AFR] END) AS ReinsuranceFlatfee6,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [ProjectItem].[Reinsurance_Flatfee] <= 84)) THEN [AFR] END) AS ReinsuranceFlatfee7,

			[ProjectItem].[Reinsurance_UpliftFactor] AS ReinsuranceUpliftFactor, 

			t.[MaterialCostWarranty],
			t.[MaterialCostOow],

			t.[Duration],

			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 12 OR (0  < [StdMonths] AND [StdMonths] <= 12)) THEN [FieldServiceCost] END) AS FieldServiceCost1,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 24 OR (12 < [StdMonths] AND [StdMonths] <= 24)) THEN [FieldServiceCost] END) AS FieldServiceCost2,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 36 OR (24 < [StdMonths] AND [StdMonths] <= 36)) THEN [FieldServiceCost] END) AS FieldServiceCost3,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 48 OR (36 < [StdMonths] AND [StdMonths] <= 48)) THEN [FieldServiceCost] END) AS FieldServiceCost4,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 60 OR (48 < [StdMonths] AND [StdMonths] <= 60)) THEN [FieldServiceCost] END) AS FieldServiceCost5,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 72 OR (60 < [StdMonths] AND [StdMonths] <= 72)) THEN [FieldServiceCost] END) AS FieldServiceCost6,
			MIN(CASE WHEN [StdIsProlongation] = 0 AND ([StdMonths] = 84 OR (72 < [StdMonths] AND [StdMonths] <= 84)) THEN [FieldServiceCost] END) AS FieldServiceCost7,

			t.[StandardHandling],
            t.[HighAvailabilityHandling],
            t.[StandardDelivery],
            t.[ExpressDelivery],
            t.[TaxiCourierDelivery],
            t.[ReturnDeliveryFactory],

			(t.[StandardHandling] + t.[HighAvailabilityHandling]) * SUM(t.[AFR]) AS LogisticsHandling,
			(t.[StandardDelivery] + t.[ExpressDelivery] + t.[TaxiCourierDelivery] + t.[ReturnDeliveryFactory]) * SUM(t.[AFR]) AS LogisticTransportcost,

			t.[Currency]
		FROM
			[Hardware].[GetCostsYear](
				@approved, 
				@cntTable, 
				@wgTable, 
				@av, 
				@duration, 
				@reactiontime, 
				@reactiontypeTable,
				@locTable,
				@proTable,
				NULL,
				NULL,
				@projectId) AS t
		INNER JOIN
			[ProjectCalculator].[ProjectItem] ON t.[Id] = [ProjectItem].[Id]
		GROUP BY
			t.[Country],
			t.[CountryId],
			t.[WgId],
			t.[ServiceLocation],
			t.[ReactionTime],
			t.[ReactionType],
			t.[Availability],

			t.[LabourCost],
			t.[TravelCost],
			t.[PerformanceRate],
			t.[TravelTime],
			t.[RepairTime],
			t.[OnsiteHourlyRates],
			t.[TimeAndMaterialShare_norm],
			t.[OohUpliftFactor],
			t.[AvailabilityFee],
			t.[TaxAndDutiesW],
			t.[MarkupOtherCost],
			t.[MarkupFactorOtherCost],
			t.[MarkupFactorStandardWarranty],
			t.[MarkupStandardWarranty],
			t.[RiskFactorStandardWarranty],
			t.[RiskStandardWarranty],
			t.[1stLevelSupportCosts],
			t.[2ndLevelSupportCosts],
			t.[Sar],
			t.[MaterialCostWarranty],
			t.[MaterialCostOow],
			t.[Duration],
			t.[StandardHandling],
            t.[HighAvailabilityHandling],
            t.[StandardDelivery],
            t.[ExpressDelivery],
            t.[TaxiCourierDelivery],
            t.[ReturnDeliveryFactory],
			t.[Currency],

			[ProjectItem].[Reinsurance_UpliftFactor]
	)
	INSERT INTO @result
	(
		[Id],
		[Country],
		[WgDescription],
		[Wg],
		[SogDescription],
		[SCD_ServiceType],
		[ServiceLocation],
		[ReactionTime],
		[ReactionType],
		[Availability],
		[Duration],
		[Currency],

		[LabourCost],
		[TravelCost],
		[PerformanceRate],
		[TravelTime],
		[RepairTime],
		[OnsiteHourlyRate],
		[TimeAndMaterialShare],
		[OohUpliftFactor],
		[AvailabilityFee],
		[TaxAndDutiesW],
		[MarkupOtherCost],
		[MarkupFactorOtherCost],
		[MarkupFactorStandardWarranty],
		[MarkupStandardWarranty],
		[RiskFactorStandardWarranty],
		[RiskStandardWarranty],
		[AFR1],
		[AFR2],
		[AFR3],
		[AFR4],
		[AFR5],
		[AFR6],
		[AFR7],
		[1stLevelSupportCosts],
		[2ndLevelSupportCosts],
		[Sar],
		[ReinsuranceFlatfee1],
		[ReinsuranceFlatfee2],
		[ReinsuranceFlatfee3],
		[ReinsuranceFlatfee4],
		[ReinsuranceFlatfee5],
		[ReinsuranceFlatfee6],
		[ReinsuranceFlatfee7],
		[ReinsuranceUpliftFactor],
		[MaterialCostWarranty],
		[MaterialCostOow],
		[FieldServiceCost1],
		[FieldServiceCost2],
		[FieldServiceCost3],
		[FieldServiceCost4],
		[FieldServiceCost5],
		[FieldServiceCost6],
		[FieldServiceCost7],
		[StandardHandling],
		[HighAvailabilityHandling],
		[StandardDelivery],
		[ExpressDelivery],
		[TaxiCourierDelivery],
		[ReturnDeliveryFactory],
		[LogisticsHandling],
		[LogisticTransportcost],
		[IB_per_Country],
		[IB_per_PLA]
	)
	SELECT 
		[Costs].[Id],
		[Costs].[Country],
		[Wg].[Description] AS WgDescription,
		[Wg].[Name] AS [Wg],
		[Sog].[Description] AS SogDescription,
		[Wg].[SCD_ServiceType],
		[Costs].[ServiceLocation],
		[Costs].[ReactionTime],
		[Costs].[ReactionType],
		[Costs].[Availability],
		[Costs].[Duration],
		[Costs].[Currency],

		[Costs].[LabourCost],
		[Costs].[TravelCost],
		[Costs].[PerformanceRate],
		[Costs].[TravelTime],
		[Costs].[RepairTime],
		[Costs].[OnsiteHourlyRates] AS OnsiteHourlyRate,
		[Costs].[TimeAndMaterialShare],
		[Costs].[OohUpliftFactor],
		[Costs].[AvailabilityFee],
		[Costs].[TaxAndDutiesW],
		[Costs].[MarkupOtherCost],
		[Costs].[MarkupFactorOtherCost],
		[Costs].[MarkupFactorStandardWarranty],
		[Costs].[MarkupStandardWarranty],
		[Costs].[RiskFactorStandardWarranty],
		[Costs].[RiskStandardWarranty],
		[Costs].[AFR1],
		[Costs].[AFR2],
		[Costs].[AFR3],
		[Costs].[AFR4],
		[Costs].[AFR5],
		[Costs].[AFR6],
		[Costs].[AFR7],
		[Costs].[1stLevelSupportCosts],
		[Costs].[2ndLevelSupportCosts],
		[Costs].[Sar],
		[Costs].[ReinsuranceFlatfee1],
		[Costs].[ReinsuranceFlatfee2],
		[Costs].[ReinsuranceFlatfee3],
		[Costs].[ReinsuranceFlatfee4],
		[Costs].[ReinsuranceFlatfee5],
		[Costs].[ReinsuranceFlatfee6],
		[Costs].[ReinsuranceFlatfee7],
		[Costs].[ReinsuranceUpliftFactor],
		[Costs].[MaterialCostWarranty],
		[Costs].[MaterialCostOow],
		[Costs].[FieldServiceCost1],
		[Costs].[FieldServiceCost2],
		[Costs].[FieldServiceCost3],
		[Costs].[FieldServiceCost4],
		[Costs].[FieldServiceCost5],
		[Costs].[FieldServiceCost6],
		[Costs].[FieldServiceCost7],
		[Costs].[StandardHandling],
		[Costs].[HighAvailabilityHandling],
		[Costs].[StandardDelivery],
		[Costs].[ExpressDelivery],
		[Costs].[TaxiCourierDelivery],
		[Costs].[ReturnDeliveryFactory],
		[Costs].[LogisticsHandling],
		[Costs].[LogisticTransportcost],
		(
			CASE 
				WHEN @approved = 0 
				THEN [ServiceSupportCostView].[Total_IB_Pla] 
				ELSE [ServiceSupportCostView].[Total_IB_Pla_Approved]
			END
		) 
		AS IB_per_PLA,
		(
			CASE 
				WHEN @approved = 0 
				THEN [ServiceSupportCostView].[TotalIb] 
				ELSE [ServiceSupportCostView].[TotalIb_Approved] 
			END
		) 
		AS IB_per_Country
	FROM
		[Costs]
	INNER JOIN	
		[InputAtoms].[Wg] ON [Costs].[WgId] = [Wg].[Id]
	INNER JOIN
		[InputAtoms].[Sog] ON [Wg].[SogId] = [Sog].[Id]
	INNER JOIN
		[InputAtoms].[Pla] ON [Wg].[PlaId] = [Pla].[Id]
	LEFT JOIN
		[Hardware].[ServiceSupportCostView] ON 
			[ServiceSupportCostView].[Country] = [Costs].[CountryId] AND
			[ServiceSupportCostView].[ClusterPla] = [Pla].[ClusterPlaId]

	RETURN
END
GO

-------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('[ProjectCalculator].[CalcParameterHw]') IS NOT NULL
  DROP FUNCTION [ProjectCalculator].[CalcParameterHw];
GO 

CREATE FUNCTION [ProjectCalculator].[CalcParameterHw]
(
    @cnt          bigint,
    @wg           bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
	@projectId    BIGINT
)
RETURNS TABLE 
AS
RETURN (
    SELECT    
                m.Id
              , m.Country
              , m.WgDescription
              , m.Wg
              , m.SogDescription
              , m.SCD_ServiceType
              , m.ServiceLocation
              , m.ReactionTime
              , m.ReactionType
              , m.Availability

              , m.LabourCost as LabourCost
              , m.TravelCost as TravelCost
              , m.PerformanceRate as PerformanceRate
              , m.TravelTime
              , m.RepairTime
              , m.OnsiteHourlyRate as OnsiteHourlyRate

              , m.TimeAndMaterialShare
              , m.OohUpliftFactor

              , m.AvailabilityFee as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
              , m.RiskFactorStandardWarranty as RiskFactorStandardWarranty
              , m.RiskStandardWarranty as RiskStandardWarranty
      
              , m.AFR1
              , m.AFR2
              , m.AFR3
              , m.AFR4
              , m.AFR5
              , m.AFR6
              , m.AFR7

              , m.[1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts]
              , m.Sar

              , m.ReinsuranceFlatfee1
              , m.ReinsuranceFlatfee2
              , m.ReinsuranceFlatfee3
              , m.ReinsuranceFlatfee4
              , m.ReinsuranceFlatfee5
              , m.ReinsuranceFlatfee6
              , m.ReinsuranceFlatfee7
              , m.ReinsuranceUpliftFactor

              , m.MaterialCostWarranty
              , m.MaterialCostOow

              , m.Duration

              , m.FieldServiceCost1
              , m.FieldServiceCost2
              , m.FieldServiceCost3
              , m.FieldServiceCost4
              , m.FieldServiceCost5
              , m.FieldServiceCost6
              , m.FieldServiceCost7

              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , m.LogisticsHandling

              , m.LogisticTransportcost
        
              , m.Currency
              , m.IB_per_Country
              , m.IB_per_PLA
    FROM 
		[ProjectCalculator].[GetParameterHw](1, @cnt, @wg, @reactiontype, @loc, @pro, @projectId) m
)
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Param-hw'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 0, @reportName, 'ProjectCalculator.CalcParameterHw', 'Calculation Parameter Overview reports for HW maintenance cost elements')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Sales Product Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SCD_ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Currency', 'Local Currency', 1, 1);
set @index = @index + 1;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LabourCost', 'Labour cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TravelCost', 'Travel cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'PerformanceRate', 'Performance rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'TravelTime', 'Travel time (MTTT)', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'RepairTime', 'Repair time (MTTR)', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TimeAndMaterialShare', 'Time And Material Share', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'OohUpliftFactor', 'OOH uplift factor', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'OnsiteHourlyRate', 'Onsite hourly rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticsHandling', 'Logistics handling cost', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticTransportcost', 'Logistics transport cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'AvailabilityFee', 'Availability Fee', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TaxAndDutiesW', 'Tax & duties', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupFactorOtherCost', 'Markup factor for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MarkupOtherCost', 'Markup for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupFactorStandardWarranty', 'Markup factor for standard warranty local cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MarkupStandardWarranty', 'Markup for standard warranty local cost', 1, 1);
set @index = @index + 1;
INSERT into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) VALUES (@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'RiskFactorStandardWarranty', 'Standard Warranty risk factor', 1, 1);
set @index = @index + 1;
INSERT into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) VALUES (@reportId, @index, Report.GetReportColumnTypeByName('money'), 'RiskStandardWarranty', 'Standard Warranty risk', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR1', 'AFR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR2', 'AFR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR3', 'AFR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR4', 'AFR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR5', 'AFR5', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR6', 'AFR6', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR7', 'AFR7', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost1', 'Calculated Field Service Cost 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost2', 'Calculated Field Service Cost 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost3', 'Calculated Field Service Cost 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost4', 'Calculated Field Service Cost 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost5', 'Calculated Field Service Cost 5 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost6', 'Calculated Field Service Cost 6 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost7', 'Calculated Field Service Cost 7 years', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), '2ndLevelSupportCosts', '2nd level support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), '1stLevelSupportCosts', '1st level support cost', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'IB_per_PLA', 'IB per Cluster PLA', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'IB_per_Country', 'IB per Country', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'Sar', 'Service Attached Rate Factor', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee1', 'Reinsurance Flatfee 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee2', 'Reinsurance Flatfee 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee3', 'Reinsurance Flatfee 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee4', 'Reinsurance Flatfee 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee5', 'Reinsurance Flatfee 5 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee6', 'Reinsurance Flatfee 6 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee7', 'Reinsurance Flatfee 7 years', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'ReinsuranceUpliftFactor', 'Reinsurance uplift factor (%)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialCostWarranty', 'Material cost iW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialCostOow', 'Material cost OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Service duration', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wghardware', 0), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');
GO

-------------------------------------------------------------------------------------------------------------------

IF OBJECT_ID('[ProjectCalculator].[CalcParameterHwNotApproved]') IS NOT NULL
  DROP FUNCTION [ProjectCalculator].[CalcParameterHwNotApproved];
GO 

CREATE FUNCTION [ProjectCalculator].[CalcParameterHwNotApproved]
(
    @cnt          bigint,
    @wg           bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
	@projectId    BIGINT
)
RETURNS TABLE 
AS
RETURN (
    SELECT    
                m.Id
              , m.Country
              , m.WgDescription
              , m.Wg
              , m.SogDescription
              , m.SCD_ServiceType
              , m.ServiceLocation
              , m.ReactionTime
              , m.ReactionType
              , m.Availability

              , m.LabourCost as LabourCost
              , m.TravelCost as TravelCost
              , m.PerformanceRate as PerformanceRate
              , m.TravelTime
              , m.RepairTime
              , m.OnsiteHourlyRate as OnsiteHourlyRate

              , m.TimeAndMaterialShare
              , m.OohUpliftFactor

              , m.AvailabilityFee as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
              , m.RiskFactorStandardWarranty as RiskFactorStandardWarranty
              , m.RiskStandardWarranty as RiskStandardWarranty
      
              , m.AFR1
              , m.AFR2
              , m.AFR3
              , m.AFR4
              , m.AFR5
              , m.AFR6
              , m.AFR7

              , m.[1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts]
              , m.Sar

              , m.ReinsuranceFlatfee1
              , m.ReinsuranceFlatfee2
              , m.ReinsuranceFlatfee3
              , m.ReinsuranceFlatfee4
              , m.ReinsuranceFlatfee5
              , m.ReinsuranceFlatfee6
              , m.ReinsuranceFlatfee7
              , m.ReinsuranceUpliftFactor

              , m.MaterialCostWarranty
              , m.MaterialCostOow

              , m.Duration

              , m.FieldServiceCost1
              , m.FieldServiceCost2
              , m.FieldServiceCost3
              , m.FieldServiceCost4
              , m.FieldServiceCost5
              , m.FieldServiceCost6
              , m.FieldServiceCost7

              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , m.LogisticsHandling

              , m.LogisticTransportcost
        
              , m.Currency
              , m.IB_per_Country
              , m.IB_per_PLA
    FROM 
		[ProjectCalculator].[GetParameterHw](0, @cnt, @wg, @reactiontype, @loc, @pro, @projectId) m
)
GO

DECLARE @reportName NVARCHAR(MAX) = 'Project-Calc-Param-hw-not-approved'
DECLARE @reportId BIGINT = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)

IF @reportId IS NULL
BEGIN
	INSERT INTO [Report].[Report]([CountrySpecific], [HasFreesedVersion], [Name], [SqlFunc], [Title])
	VALUES
		(1, 0, @reportName, 'ProjectCalculator.CalcParameterHwNotApproved', 'Calculation Parameter Overview reports for HW maintenance cost elements (not approved)')

	SET @reportId = (SELECT [Id] FROM [Report].[Report] WHERE [Name] = @reportName)
END

declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Sales Product Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SCD_ServiceType', 'Service Types', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level Description', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Currency', 'Local Currency', 1, 1);
set @index = @index + 1;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LabourCost', 'Labour cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TravelCost', 'Travel cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'PerformanceRate', 'Performance rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'TravelTime', 'Travel time (MTTT)', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'RepairTime', 'Repair time (MTTR)', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TimeAndMaterialShare', 'Time And Material Share', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'OohUpliftFactor', 'OOH uplift factor', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'OnsiteHourlyRate', 'Onsite hourly rate', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticsHandling', 'Logistics handling cost', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'LogisticTransportcost', 'Logistics transport cost', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'AvailabilityFee', 'Availability Fee', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'TaxAndDutiesW', 'Tax & duties', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupFactorOtherCost', 'Markup factor for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MarkupOtherCost', 'Markup for other cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'MarkupFactorStandardWarranty', 'Markup factor for standard warranty local cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MarkupStandardWarranty', 'Markup for standard warranty local cost', 1, 1);
set @index = @index + 1;
INSERT into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) VALUES (@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'RiskFactorStandardWarranty', 'Standard Warranty risk factor', 1, 1);
set @index = @index + 1;
INSERT into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) VALUES (@reportId, @index, Report.GetReportColumnTypeByName('money'), 'RiskStandardWarranty', 'Standard Warranty risk', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR1', 'AFR1', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR2', 'AFR2', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR3', 'AFR3', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR4', 'AFR4', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR5', 'AFR5', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR6', 'AFR6', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'AFR7', 'AFR7', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost1', 'Calculated Field Service Cost 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost2', 'Calculated Field Service Cost 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost3', 'Calculated Field Service Cost 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost4', 'Calculated Field Service Cost 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost5', 'Calculated Field Service Cost 5 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost6', 'Calculated Field Service Cost 6 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'FieldServiceCost7', 'Calculated Field Service Cost 7 years', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), '2ndLevelSupportCosts', '2nd level support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), '1stLevelSupportCosts', '1st level support cost', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'IB_per_PLA', 'IB per Cluster PLA', 1, 1);
set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('number'), 'IB_per_Country', 'IB per Country', 1, 1);

set @index = @index + 1;                                                                                          
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'Sar', 'Service Attached Rate Factor', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee1', 'Reinsurance Flatfee 1 year', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee2', 'Reinsurance Flatfee 2 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee3', 'Reinsurance Flatfee 3 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee4', 'Reinsurance Flatfee 4 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee5', 'Reinsurance Flatfee 5 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee6', 'Reinsurance Flatfee 6 years', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'ReinsuranceFlatfee7', 'Reinsurance Flatfee 7 years', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('percent'), 'ReinsuranceUpliftFactor', 'Reinsurance uplift factor (%)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialCostWarranty', 'Material cost iW', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('money'), 'MaterialCostOow', 'Material cost OOW', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Duration', 'Service duration', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country', 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wghardware', 0), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');

-------------------------------------------------------------------------------------------------------------------

