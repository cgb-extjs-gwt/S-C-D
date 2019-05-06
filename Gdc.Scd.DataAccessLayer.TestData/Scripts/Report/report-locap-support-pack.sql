IF OBJECT_ID('Report.spLocapGlobalSupport') IS NOT NULL
  DROP PROCEDURE Report.spLocapGlobalSupport;
go 

CREATE PROCEDURE [Report].[spLocapGlobalSupport]
(
    @cnt                bigint,
    @wg      dbo.ListID readonly,
    @av      dbo.ListID readonly,
    @dur     dbo.ListID readonly,
    @rtime   dbo.ListID readonly,
    @rtype   dbo.ListID readonly,
    @loc     dbo.ListID readonly,
    @pro     dbo.ListID readonly,
    @lastid  int,
    @limit   int
)
AS
BEGIN

    declare @cntTable dbo.ListId; insert into @cntTable(id) values(@cnt);

    declare @isEmeia bit = (select cr.IsEmeia
                                from InputAtoms.Country c 
                                join InputAtoms.ClusterRegion cr on cr.id = c.ClusterRegionId
                                where c.Id = @cnt);

    if @isEmeia = 1
    begin

        --Calc by SOG

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

        with cte as (
            select m.* 
                   , case when m.IsProlongation = 1 then 'Prolongation' else CAST(m.Year as varchar(1)) end as ServicePeriod
            from Hardware.GetCostsSlaSog(1, @cntTable, @wg_SOG_Table, @av, @dur, @rtime, @rtype, @loc, @pro) m
            where (not exists(select 1 from @wg) or exists(select 1 from @wg where id = m.WgId))
        )
        , cte2 as (
            select  
                    ROW_NUMBER() over(ORDER BY (SELECT 1)) as rownum

                    , m.*
                    , cnt.ISO3CountryCode
                    , fsp.Name as Fsp
                    , fsp.ServiceDescription as FspDescription

                    , sog.SogDescription

            from cte m
            inner join InputAtoms.Country cnt on cnt.id = m.CountryId
            inner join InputAtoms.WgSogView sog on sog.Id = m.WgId
            left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
        )
        select    c.Country
                , c.ISO3CountryCode
                , c.Fsp
                , c.FspDescription

                , c.SogDescription
                , c.Sog        
                , c.Wg

                , c.ServiceLocation
                , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
                , case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
                , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

                , c.LocalServiceStandardWarranty
                , c.ServiceTpSog ServiceTP
                , c.DealerPrice
                , c.ListPrice

        from cte2 c

        where (@limit is null) or (c.rownum > @lastid and c.rownum <= @lastid + @limit);

    end
    else
    begin
        --Calc by WG

        select    c.Country
                , cnt.ISO3CountryCode
                , fsp.Name as Fsp
                , fsp.ServiceDescription as FspDescription

                , sog.SogDescription
                , c.Sog        
                , c.Wg

                , c.ServiceLocation
                , c.ReactionTime + ' ' + c.ReactionType + ' time, ' + c.Availability as ReactionTime
                , case when c.IsProlongation = 1 then 'Prolongation' else CAST(c.Year as varchar(1)) end as ServicePeriod
                , LOWER(c.Duration) + ' ' + c.ServiceLocation as ServiceProduct

                , c.LocalServiceStandardWarranty
                , coalesce(ServiceTPManual, ServiceTP) ServiceTP
                , c.DealerPrice
                , c.ListPrice

        from Hardware.GetCosts(1, @cntTable, @wg, @av, @dur, @rtime, @rtype, @loc, @pro, @lastid, @limit) c
        inner join InputAtoms.Country cnt on cnt.id = c.CountryId
        inner join InputAtoms.WgSogView sog on sog.Id = c.WgId
        left join Fsp.HwFspCodeTranslation fsp on fsp.SlaHash = c.SlaHash and fsp.Sla = c.Sla

    end

END
GO


declare @reportId bigint = (select Id from Report.Report where upper(Name) = ('Locap-Global-Support'));
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'FspDescription', 'Portfolio Alignment', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ISO3CountryCode', 'Country specific order code', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'SogDescription', 'Service Offering Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceLocation', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Sog', 'SOG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('text'), 'ServiceProduct', 'Service Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'LocalServiceStandardWarranty', 'Standard Warranty cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ServiceTP', 'Service TP (Full cost)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'DealerPrice', 'Dealer Price (local input) for non-FTS countries', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, Report.GetReportColumnTypeByName('euro'), 'ListPrice', 'List Price (local input) for non-FTS countries', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('country'         , 0), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('wg'              , 1), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability'    , 1), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('duration'        , 1), 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontime'    , 1), 'rtime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype'    , 1), 'rtype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation' , 1), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive'       , 1), 'pro', 'ProActive');

GO

