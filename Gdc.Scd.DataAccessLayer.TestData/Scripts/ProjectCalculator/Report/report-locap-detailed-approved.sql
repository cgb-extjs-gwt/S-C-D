USE [SCD_2]

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

    declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId; if @dur is not null insert into @durTable(id) values(@dur);

    declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; insert into @proTable(id) select id from Dependencies.ProActiveSla where UPPER(ExternalName) = 'NONE';

    with cte as (
        select m.* 
               , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
        from Hardware.GetCostsSlaSogAgregated(1, @cntTable, @wg_SOG_Table, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @projectId) m
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