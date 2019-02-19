IF OBJECT_ID('Report.HwCalcResult') IS NOT NULL
  DROP FUNCTION Report.HwCalcResult;
go 

CREATE FUNCTION Report.HwCalcResult
(
	@approved bit,
    @local bit,
    @country dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @availability dbo.ListID readonly,
    @duration dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @servicelocation dbo.ListID readonly,
    @proactive dbo.ListID readonly
)
RETURNS TABLE 
AS
RETURN (
    select    Country
            , case when @local = 1 then c.Currency else 'EUR' end as Currency

            , Wg
            , Availability
            , Duration
            , ReactionTime
            , ReactionType
            , ServiceLocation
            , ProActiveSla

            , StdWarranty

            --Cost

            , case when @local = 1 then AvailabilityFee * er.Value else AvailabilityFee end as AvailabilityFee 
            , case when @local = 1 then TaxAndDutiesW * er.Value else TaxAndDutiesW end as TaxAndDutiesW
            , case when @local = 1 then TaxAndDutiesOow * er.Value else TaxAndDutiesOow end as TaxAndDutiesOow
            , case when @local = 1 then Reinsurance * er.Value else Reinsurance end as Reinsurance
            , case when @local = 1 then ProActive * er.Value else ProActive end as ProActive
            , case when @local = 1 then ServiceSupportCost * er.Value else ServiceSupportCost end as ServiceSupportCost
                                                          
            , case when @local = 1 then MaterialW * er.Value else MaterialW end as MaterialW
            , case when @local = 1 then MaterialOow * er.Value else MaterialOow end as MaterialOow
            , case when @local = 1 then FieldServiceCost * er.Value else FieldServiceCost end as FieldServiceCost
            , case when @local = 1 then Logistic * er.Value else Logistic end as Logistic
            , case when @local = 1 then OtherDirect * er.Value else OtherDirect end as OtherDirect
            , case when @local = 1 then LocalServiceStandardWarranty * er.Value else LocalServiceStandardWarranty end as LocalServiceStandardWarranty
            , case when @local = 1 then Credits * er.Value else Credits end as Credits
            , case when @local = 1 then ServiceTC * er.Value else ServiceTC end as ServiceTC
            , case when @local = 1 then ServiceTP * er.Value else ServiceTP end as ServiceTP
                                                          
            , case when @local = 1 then ServiceTCManual * er.Value else ServiceTCManual end as ServiceTCManual
            , case when @local = 1 then ServiceTPManual * er.Value else ServiceTPManual end as ServiceTPManual
                                                          
            , case when @local = 1 then ServiceTP_Released * er.Value else ServiceTP_Released end as ServiceTP_Released
                                                          
            , case when @local = 1 then ListPrice * er.Value else ListPrice end as ListPrice
            , case when @local = 1 then DealerPrice * er.Value else DealerPrice end as DealerPrice
            , DealerDiscount                               as DealerDiscount
                                                           
            , ChangeUserName + '[' + ChangeUserEmail + ']' as ChangeUser

    from Hardware.GetCosts(@approved, @country, @wg, @availability, @duration, @reactiontime, @reactiontype, @servicelocation, @proactive, -1, -1) costs
    join InputAtoms.CountryView c on c.Name = costs.Country
    join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HW-CALC-RESULT');
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
declare @filterTypeId bigint = 0

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'boolean'), 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'boolean'), 'local', 'Local currency');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'country' and MultiSelect=1), 'country', 'Country');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'wg' and MultiSelect=1), 'wg', 'Asset(WG)');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'availability' and MultiSelect=1), 'availability', 'Availability');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'duration' and MultiSelect=1), 'duration', 'Duration');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'reactiontime' and MultiSelect=1), 'reactiontime', 'Reaction time');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'reactiontype' and MultiSelect=1), 'reactiontype', 'Reaction type');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'servicelocation' and MultiSelect=1), 'servicelocation', 'Service location');

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select Id from Report.ReportFilterType where Name = 'proactive' and MultiSelect=1), 'proactive', 'ProActive');



