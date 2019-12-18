if OBJECT_ID('[Report].[spContract]') is not null
    drop procedure [Report].[spContract];
go

CREATE PROCEDURE [Report].[spContract]
(
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc          bigint,
    @pro          bigint,
    @lastid       bigint,
    @limit        int
)
AS
BEGIN


    declare @avTable dbo.ListId; if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId; insert into @durTable(id) select id from Dependencies.Duration where IsProlongation = 0 and Value = 5;

    declare @rtimeTable dbo.ListId; if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId; if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId; if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId; if @pro is not null insert into @proTable(id) values(@pro);

    declare @countries table (
          Id bigint not null
        , Name nvarchar(128)
        , Currency nvarchar(16)
        , ExchangeRate float
    );

    insert into @countries
    select c.Id, c.Name, cur.Name as Currency, er.Value as ExchangeRate
    from InputAtoms.Country c 
    left join [References].Currency cur on cur.Id = c.CurrencyId
    left join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId

    where exists(select * from @cnt where Id = c.Id);

    select 
           m.Id
         , c.Name              as Country
         , wg.Name             as Wg
         , wg.Description      as WgDescription
         , loc.Name            as ServiceLocation
         , rtime.Name          as ReactionTime
         , rtype.Name          as ReactionType
         , av.Name             as Availability
         , prosla.ExternalName as ProActiveSla

         , mc.ServiceTP1_Released * c.ExchangeRate      as ServiceTP1
         , mc.ServiceTP2_Released * c.ExchangeRate      as ServiceTP2
         , mc.ServiceTP3_Released * c.ExchangeRate      as ServiceTP3
         , mc.ServiceTP4_Released * c.ExchangeRate      as ServiceTP4
         , mc.ServiceTP5_Released * c.ExchangeRate      as ServiceTP5

         , mc.ServiceTP1_Released * c.ExchangeRate / 12 as ServiceTPMonthly1
         , mc.ServiceTP2_Released * c.ExchangeRate / 12 as ServiceTPMonthly2
         , mc.ServiceTP3_Released * c.ExchangeRate / 12 as ServiceTPMonthly3
         , mc.ServiceTP4_Released * c.ExchangeRate / 12 as ServiceTPMonthly4
         , mc.ServiceTP5_Released * c.ExchangeRate / 12 as ServiceTPMonthly5
         , c.Currency
       
          , stdw.DurationValue as WarrantyLevel
          , wg.ServiceTypes               as PortfolioType
          , wg.Sog             as Sog

    FROM Portfolio.GetBySlaPaging(@cnt, @wg, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, @lastId, @limit) m

    INNER JOIN @countries c on c.Id = m.CountryId

    INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Fsp.HwStandardWarranty stdw ON stdw.Country = m.CountryId and stdw.Wg = m.WgId

    LEFT JOIN Hardware.ManualCost mc on mc.PortfolioId = m.Id;

end
go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CONTRACT');
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
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('availability', 0), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontime', 0), 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('reactiontype', 0), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('servicelocation', 0), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, Report.GetReportFilterTypeByName('proactive', 0), 'pro', 'ProActive');

