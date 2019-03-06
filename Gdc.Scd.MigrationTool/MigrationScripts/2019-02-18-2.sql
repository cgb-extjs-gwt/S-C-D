IF OBJECT_ID('Report.Locap') IS NOT NULL
  DROP FUNCTION Report.Locap;
go 

CREATE FUNCTION Report.Locap
(
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    select m.Id
         , m.Fsp
         , wg.Description as WgDescription
         , m.FspDescription as ServiceLevel

         , m.ReactionTime
         , m.Year as ServicePeriod
         , wg.Name as Wg
         , m.LocalServiceStandardWarranty
         , m.ServiceTC
         , m.ServiceTP_Released
         , m.Country

         , m.Availability                       + ', ' +
               m.ReactionType                   + ', ' +
               m.ReactionTime                   + ', ' +
               cast(m.Year as nvarchar(1))      + ', ' +
               m.ServiceLocation                + ', ' +
               m.ProActiveSla as ServiceType

         , null as PlausiCheck
         , null as PortfolioType
         , null as ReleaseCreated
         , wg.Sog
    from Report.GetCosts(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro) m
    join InputAtoms.WgSogView wg on wg.id = m.WgId
)
GO

declare @reportId bigint = (select Id from Report.Report where Name = 'Locap');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLevel', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServicePeriod', 'Service Period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'LocalServiceStandardWarranty', 'Standard Warranty costs', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTC', 'Service TC', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ServiceTP_Released', 'Service TP (Released)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceType', 'Service type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PlausiCheck', 'Plausi Check', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'PortfolioType', 'Portfolio Type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReleaseCreated', 'Release created', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='country'         ), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='wg'              ), 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='availability'    ), 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='duration'        ), 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='reactiontime'    ), 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='reactiontype'    ), 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='servicelocation' ), 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where MultiSelect = 0 and name ='proactive'       ), 'pro', 'ProActive');

GO

ALTER FUNCTION [Report].[GetCostsFull](
  @cnt bigint,
    @wg bigint,
    @av bigint,
    @dur bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS @tbl TABLE (
           Fsp nvarchar(max) NULL
         , FspDescription nvarchar(max) NULL
           
         , Id bigint NOT NULL
           
         , CountryId bigint NOT NULL
         , Country nvarchar(max) NULL
         , WgId bigint NOT NULL
         , Wg nvarchar(max) NULL
         , AvailabilityId bigint NOT NULL
         , Availability nvarchar(max) NULL
         , DurationId bigint NOT NULL
         , Duration nvarchar(max) NULL
         , Year int NOT NULL
         , IsProlongation bit NOT NULL
         , ReactionTimeId bigint NOT NULL
         , ReactionTime nvarchar(max) NULL
         , ReactionTypeId bigint NOT NULL
         , ReactionType nvarchar(max) NULL
         , ServiceLocationId bigint NOT NULL
         , ServiceLocation nvarchar(max) NULL
         , ProActiveSlaId bigint NOT NULL
         , ProActiveSla nvarchar(max) NULL
           
         , StdWarranty int NULL
           
         --Cost
           
         , AvailabilityFee float NULL
         , TaxAndDutiesW float NULL
         , TaxAndDutiesOow float NULL
         , Reinsurance float NULL
         , ProActive float NULL
         , ServiceSupportCost float NULL
           
         , MaterialW float NULL
         , MaterialOow float NULL
         , FieldServiceCost float NULL
         , Logistic float NULL
         , OtherDirect float NULL
         
         , LocalServiceStandardWarranty float NULL
         
         , Credits float NULL

         , ServiceTC float NULL
         , ServiceTP float NULL

         , ServiceTC1 float NULL
         , ServiceTC2 float NULL
         , ServiceTC3 float NULL
         , ServiceTC4 float NULL
         , ServiceTC5 float NULL
         , ServiceTC1P float NULL
           
         , ServiceTP1 float NULL
         , ServiceTP2 float NULL
         , ServiceTP3 float NULL
         , ServiceTP4 float NULL
         , ServiceTP5 float NULL
         , ServiceTP1P float NULL
           
         , ListPrice float NULL
         , DealerDiscount float NULL
         , DealerPrice float NULL
         , ServiceTCManual float NULL
         , ServiceTPManual float NULL
         , ChangeUserName nvarchar(max) NULL
         , ChangeUserEmail nvarchar(max) NULL
           
         , ServiceTP_Released float NULL
           
         , SlaHash int NOT NULL
) 
AS
begin
    declare @cntTable dbo.ListId;
    if @cnt is not null insert into @cntTable(id) values(@cnt);

    declare @wgTable dbo.ListId;
    if @wg is not null insert into @wgTable(id) values(@wg);

    declare @avTable dbo.ListId;
    if @av is not null insert into @avTable(id) values(@av);

    declare @durTable dbo.ListId;
    if @dur is not null insert into @durTable(id) values(@dur);

    declare @rtimeTable dbo.ListId;
    if @reactiontime is not null insert into @rtimeTable(id) values(@reactiontime);

    declare @rtypeTable dbo.ListId;
    if @reactiontype is not null insert into @rtypeTable(id) values(@reactiontype);

    declare @locTable dbo.ListId;
    if @loc is not null insert into @locTable(id) values(@loc);

    declare @proTable dbo.ListId;
    if @pro is not null insert into @proTable(id) values(@pro);

    insert into @tbl
    select 
           fsp.Name as Fsp
         , fsp.ServiceDescription as FspDescription

         ,m.*

    FROM Hardware.GetCostsFull(1, @cntTable, @wgTable, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, 0, -1) m
    LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                           and fsp.CountryId = m.CountryId
                                           and fsp.WgId = m.WgId
                                           and fsp.AvailabilityId = m.AvailabilityId
                                           and fsp.DurationId= m.DurationId
                                           and fsp.ReactionTimeId = m.ReactionTimeId
                                           and fsp.ReactionTypeId = m.ReactionTypeId
                                           and fsp.ServiceLocationId = m.ServiceLocationId
                                           and fsp.ProactiveSlaId = m.ProActiveSlaId

return

end

go
