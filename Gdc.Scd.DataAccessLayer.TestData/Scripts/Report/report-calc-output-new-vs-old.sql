IF OBJECT_ID('Report.CalcOutputNewVsOld') IS NOT NULL
  DROP FUNCTION Report.CalcOutputNewVsOld;
go 

CREATE FUNCTION Report.CalcOutputNewVsOld
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
RETURNS @tbl TABLE (
	Id bigint NOT NULL
	,Country nvarchar(max) NULL
  ,SogDescription nvarchar(max) NULL
  ,Name nvarchar(max) NULL
  ,WgDescription nvarchar(max) NULL
  ,ServiceLocation nvarchar(max) NULL
  ,ReactionTime nvarchar(max) 
  ,Wg nvarchar(max) NULL
  ,ServiceProduct nvarchar(max) NULL
  ,LocalServiceStandardWarranty nvarchar(max) NULL
  ,StandardWarrantyOld nvarchar(max) NULL
  ,Sog nvarchar(max) NULL
  ,Bw float NULL
)
AS
begin
	declare @cntTable dbo.ListId;
	if @cnt is not null insert into @cntTable(id) SELECT id FROM Portfolio.IntToListID(@cnt);

	declare @wgTable dbo.ListId;
	if @wg is not null insert into @wgTable(id) SELECT id FROM Portfolio.IntToListID(@wg);

	declare @avTable dbo.ListId;
	if @av is not null insert into @avTable(id) SELECT id FROM Portfolio.IntToListID(@av);

	declare @durTable dbo.ListId;
	if @dur is not null insert into @durTable(id) SELECT id FROM Portfolio.IntToListID(@dur);

	declare @rtimeTable dbo.ListId;
	if @reactiontime is not null insert into @rtimeTable(id) SELECT id FROM Portfolio.IntToListID(@reactiontime);
	
	declare @rtypeTable dbo.ListId;
	if @reactiontype is not null insert into @rtypeTable(id) SELECT id FROM Portfolio.IntToListID(@reactiontype);
	
	declare @locTable dbo.ListId;
	if @loc is not null insert into @locTable(id) SELECT id FROM Portfolio.IntToListID(@loc);
	
	declare @proTable dbo.ListId;
	if @pro is not null insert into @proTable(id) SELECT id FROM Portfolio.IntToListID(@pro);

	insert into @tbl
    select    m.Id
            , m.Country 
            , wg.SogDescription
            , fsp.Name
            , wg.Description as WgDescription
            , m.ServiceLocation
            , m.ReactionTime
            , m.Wg
         
            , (m.Duration + ' ' + m.ServiceLocation) as ServiceProduct
            
            , m.LocalServiceStandardWarranty
            , null as StandardWarrantyOld

            , wg.Sog

            , (100 * (m.LocalServiceStandardWarranty - null) / m.LocalServiceStandardWarranty) as Bw

    FROM Hardware.GetCostsFull(0, @cntTable, @wgTable, @avTable, @durTable, @rtimeTable, @rtypeTable, @locTable, @proTable, 0, -1) m --not approved

    INNER JOIN InputAtoms.WgSogView wg on wg.id = m.WgId

    LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash 
                                           and fsp.CountryId = m.CountryId
                                           and fsp.WgId = m.WgId
                                           and fsp.AvailabilityId = m.AvailabilityId
                                           and fsp.DurationId= m.DurationId
                                           and fsp.ReactionTimeId = m.ReactionTimeId
                                           and fsp.ReactionTypeId = m.ReactionTypeId
                                           and fsp.ServiceLocationId = m.ServiceLocationId
                                           and fsp.ProactiveSlaId = m.ProActiveSlaId
return;
end
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCOUTPUT-NEW-VS-OLD');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'SogDescription', 'Portfolio Alignment', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Product_No', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service Level', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction Time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceProduct', 'Service Product', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'StandardWarranty', 'NEW - Service standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'StandardWarrantyOld', 'OLD - Service standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'Bw', 'b/w %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'Sog', 'SOG', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'av', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 9, 'dur', 'Service period');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'loc', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'pro', 'ProActive');

GO