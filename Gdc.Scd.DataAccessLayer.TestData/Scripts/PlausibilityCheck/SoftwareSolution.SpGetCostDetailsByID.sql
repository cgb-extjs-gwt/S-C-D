if OBJECT_ID('SoftwareSolution.SpGetCostDetailsByID') is not null
    drop procedure SoftwareSolution.SpGetCostDetailsByID;
go

create procedure SoftwareSolution.SpGetCostDetailsByID(
    @approved       bit , 
    @id             bigint
)
as
begin

    declare @germany nvarchar(32) = 'Germany';
    declare @central nvarchar(64) = 'Central';
    declare @av nvarchar(32);
    declare @durAv nvarchar(32);
    declare @1stLevelSupportCosts float;
    declare @2ndLevelSupportCosts float;
    declare @InstalledBaseSog float;
    declare @TotalIb float;
    declare @ReinsuranceFlatfee float;
    declare @CurrencyReinsurance nvarchar(10);
    declare @RecommendedSwSpMaintenanceListPrice  float;
    declare @ShareSwSpMaintenanceListPrice float;
    declare @MarkupForProductMarginSwLicenseListPrice float;
    declare @DiscountDealerPrice float;

    declare @tbl table (
          Mandatory       bit default(1)
        , CostBlock       nvarchar(64)
        , CostElement     nvarchar(64)
        , Value           nvarchar(64)
        , Dependency      nvarchar(64)
        , Level           nvarchar(64)
        , [order]         int identity
    );

    SELECT top(1)
              @1stLevelSupportCosts = case when @approved = 0 then ssc.[1stLevelSupportCostsCountry] else ssc.[1stLevelSupportCostsCountry_Approved] end 
            , @TotalIb = case when @approved = 0 then ssc.TotalIb else TotalIb_Approved end 
    FROM Hardware.ServiceSupportCost ssc
    JOIN InputAtoms.Country c on c.Id = ssc.Country and c.ISO3CountryCode = 'DEU' --install base by Germany!

    select  @2ndLevelSupportCosts = case when @approved = 0 then ssm.[2ndLevelSupportCosts] else ssm.[2ndLevelSupportCosts_Approved] end
          , @InstalledBaseSog = case when @approved = 0 then ssm.InstalledBaseSog else ssm.InstalledBaseSog_Approved end
          , @ReinsuranceFlatfee = case when @approved = 0 then ssm.ReinsuranceFlatfee else ssm.ReinsuranceFlatfee_Approved end
          , @CurrencyReinsurance = (select Name from [References].Currency where id = case when @approved = 0 then ssm.CurrencyReinsurance else ssm.CurrencyReinsurance_Approved end)

          , @RecommendedSwSpMaintenanceListPrice = case when @approved = 0 then ssm.RecommendedSwSpMaintenanceListPrice else ssm.RecommendedSwSpMaintenanceListPrice_Approved end
          , @ShareSwSpMaintenanceListPrice = case when @approved = 0 then ssm.ShareSwSpMaintenanceListPrice else ssm.ShareSwSpMaintenanceListPrice_Approved end

          , @MarkupForProductMarginSwLicenseListPrice = case when @approved = 0 then ssm.MarkupForProductMarginSwLicenseListPrice else ssm.MarkupForProductMarginSwLicenseListPrice_Approved end
          , @DiscountDealerPrice = case when @approved = 0 then ssm.DiscountDealerPrice else ssm.DiscountDealerPrice_Approved end

          , @durAv = (select Name from Dependencies.DurationAvailability where id = ssm.DurationAvailability)
          , @av = (select name from Dependencies.Availability where id = (select AvailabilityId from Dependencies.Duration_Availability where Id = ssm.DurationAvailability))
    from SoftwareSolution.SwSpMaintenance ssm
    where ssm.Id = @id;

    insert into @tbl values
            (1, 'Service support cost', '1st level Support costs country', FORMAT(@1stLevelSupportCosts, '') + ' EUR', null, @germany)
          , (1, 'Service support cost', 'Installed base per country', FORMAT(@TotalIb, ''), null, @germany)
          , (1, 'SW / SP Maintenance', '2nd Level Support costs local', FORMAT(@2ndLevelSupportCosts, '') + ' EUR', null, @central)
          , (1, 'SW / SP Maintenance', 'Installed base', FORMAT(@InstalledBaseSog, ''), null, @central)
          , (0, 'SW / SP Maintenance', 'Reinsurance Flatfee', FORMAT(@ReinsuranceFlatfee, '') + ' ' + @CurrencyReinsurance, @durAv, @central)
          , (0, 'SW / SP Maintenance', 'SW/SP Maintenance List Price', FORMAT(@RecommendedSwSpMaintenanceListPrice, '') + ' EUR', @durAv, @central)
          , (0, 'SW / SP Maintenance', 'Share Reinsurance of SW/SP Maintenance List Price', FORMAT(@ShareSwSpMaintenanceListPrice, '') + ' %', @durAv, @central)
          , (0, 'SW / SP Maintenance', 'Markup for Product Margin of SW License List Price', FORMAT(@MarkupForProductMarginSwLicenseListPrice, '') + ' %', @av, @central)
          , (1, 'SW / SP Maintenance', 'Discount to Dealer price', FORMAT(@DiscountDealerPrice, '') + ' %', null, @central)

    --##########################################

    select CostBlock, CostElement, Dependency, Level, Value, Mandatory
    from @tbl order by [order];

end
go

exec SoftwareSolution.SpGetCostDetailsByID 0, 657;



