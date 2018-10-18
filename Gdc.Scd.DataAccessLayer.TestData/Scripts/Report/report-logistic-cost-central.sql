IF OBJECT_ID('Report.LogisticCostCentral') IS NOT NULL
  DROP FUNCTION Report.LogisticCostCentral;
go 

CREATE FUNCTION Report.LogisticCostCentral
(
    @cnt bigint,
    @wg bigint,
    @reactiontime bigint,
    @reactiontype bigint
)
RETURNS TABLE 
AS
RETURN (
    select c.Region
         , c.Name as Country
         , wg.Name as Wg

         , 'EUR' as Currency

         , (time.Name + ' ' + type.Name) as ReactionType

         , l.StandardHandling_Approved as StandardHandling
         , l.HighAvailabilityHandling_Approved as HighAvailabilityHandling
         , l.StandardDelivery_Approved as StandardDelivery
         , l.ExpressDelivery_Approved as ExpressDelivery
         , l.TaxiCourierDelivery_Approved as TaxiCourierDelivery
         , l.ReturnDeliveryFactory_Approved as ReturnDeliveryFactory

    from Hardware.LogisticsCostView l
    join InputAtoms.CountryView c on c.Id = l.Country
    join InputAtoms.WgSogView wg on wg.Id = l.Wg
    JOIN Dependencies.ReactionTime time ON time.id = l.ReactionTime
    JOIN Dependencies.ReactionType type ON type.Id = l.ReactionType

    where (@cnt is null or l.Country = @cnt)
      and (@wg is null or l.Wg = @wg)
      and (@reactiontime is null or l.ReactionTime = @reactiontime)
      and (@reactiontype is null or l.ReactionType = @reactiontype)

)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'LOGISTIC-COST-CENTRAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Region', 'Alias Region', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Currency', 'Input Currency', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction Type', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardHandling', 'Standard handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'HighAvailabilityHandling', 'High availability handling', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StandardDelivery', 'Standard delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ExpressDelivery', 'Express delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TaxiCourierDelivery', 'Taxi courier delivery', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReturnDeliveryFactory', 'Return delivery factory', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 7, 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Warranty Group');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');

