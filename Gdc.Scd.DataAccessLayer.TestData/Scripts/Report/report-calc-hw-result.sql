IF OBJECT_ID('Report.CalcHwResult') IS NOT NULL
  DROP FUNCTION Report.CalcHwResult;
go 

CREATE FUNCTION Report.CalcHwResult
(
    @approved bit,
    @local bit,
    @country bigint,
    @wg bigint,
    @availability bigint,
    @duration bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @servicelocation bigint,
    @proactive bigint
)
RETURNS TABLE 
AS
RETURN (
    with CurrencyCte as (
        select   case when @local = 1 then cur.Name else 'EUR' end as Currency
               , case when @local = 1 then er.Value else 1     end as Exchange
        from [References].Currency cur
        join [References].ExchangeRate er on er.CurrencyId = cur.Id
        where cur.Id = (select CurrencyId from InputAtoms.Country where id = @country)
    )
    select    Country
            , Currency

            , Wg
            , Availability
            , Duration
            , ReactionTime
            , ReactionType
            , ServiceLocation
            , ProActiveSla

            , StdWarranty

            --Cost

            , AvailabilityFee               * Exchange     as AvailabilityFee 
            , HddRet                        * Exchange     as HddRet
            , TaxAndDutiesW                 * Exchange     as TaxAndDutiesW
            , TaxAndDutiesOow               * Exchange     as TaxAndDutiesOow
            , Reinsurance                   * Exchange     as Reinsurance
            , ProActive                     * Exchange     as ProActive
            , ServiceSupportCost            * Exchange     as ServiceSupportCost
                                                          
            , MaterialW                     * Exchange     as MaterialW
            , MaterialOow                   * Exchange     as MaterialOow
            , FieldServiceCost              * Exchange     as FieldServiceCost
            , Logistic                      * Exchange     as Logistic
            , OtherDirect                   * Exchange     as OtherDirect
            , LocalServiceStandardWarranty  * Exchange     as LocalServiceStandardWarranty
            , Credits                       * Exchange     as Credits
            , ServiceTC                     * Exchange     as ServiceTC
            , ServiceTP                     * Exchange     as ServiceTP
                                                          
            , ServiceTCManual               * Exchange     as ServiceTCManual
            , ServiceTPManual               * Exchange     as ServiceTPManual
                                                          
            , ServiceTP_Released            * Exchange     as ServiceTP_Released
                                                          
            , ListPrice                     * Exchange     as ListPrice
            , DealerPrice                   * Exchange     as DealerPrice
            , DealerDiscount                               as DealerDiscount
                                                           
            , ChangeUserName + '[' + ChangeUserEmail + ']' as ChangeUser

    from Hardware.GetCosts(@approved, @country, @wg, @availability, @duration, @reactiontime, @reactiontype, @servicelocation, @proactive, -1, -1), CurrencyCte
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'CALCULATION-HW-RESULT');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

declare @money bigint;
select @money = id from Report.ReportColumnType where upper(name) = 'MONEY';

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG(Asset)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Availability', 'Availability', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Duration', 'Duration', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionType', 'Reaction type', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ReactionTime', 'Reaction time', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ServiceLocation', 'Service location', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ProActiveSla', 'ProActive SLA', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'StdWarranty', 'Standard warranty duration', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'FieldServiceCost', 'Field service cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceSupportCost', 'Service support cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'Logistic', 'Logistic cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'AvailabilityFee', 'Availability fee', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'HddRet', 'HDD retention', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'Reinsurance', 'Reinsurance', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'TaxAndDutiesW', 'Tax & Duties iW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'TaxAndDutiesOow', 'Tax & Duties OOW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'MaterialW', 'Material cost iW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'MaterialOow', 'Material cost OOW period', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ProActive', 'ProActive', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTC', 'Service TC(calc)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTCManual', 'Service TC(manual)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTP', 'Service TP(calc)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTPManual', 'Service TP(manual)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ServiceTP_Released', 'Service TP(released)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'ListPrice', 'List price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'DealerDiscount', 'Dealer discount in %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'DealerPrice', 'Dealer price', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ChangeUser', 'Change user', 1, 1);


set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'OtherDirect', 'Other direct cost', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'LocalServiceStandardWarranty', 'Local service standard warranty', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, @money, 'Credits', 'Credits', 1, 1);

------------------------------------
set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'local', 'Local currency');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 15, 'country', 'Country');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 4, 'wg', 'Asset(WG)');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'availability', 'Availability');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 8, 'duration', 'Duration');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 10, 'reactiontime', 'Reaction time');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 11, 'reactiontype', 'Reaction type');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 12, 'servicelocation', 'Service location');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 14, 'proactive', 'ProActive');



