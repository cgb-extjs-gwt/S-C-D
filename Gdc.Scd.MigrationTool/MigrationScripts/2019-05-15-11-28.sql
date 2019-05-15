ALTER VIEW [Hardware].[AvailabilityFeeCalcView] as 
     with InstallByCountryCte as (
        SELECT T.Country as CountryID, 
               T.Total_IB, 
               T.Total_IB_Approved, 

               T.Total_IB - coalesce(T.Total_IB_MVS, 0) as Total_IB_FTS, 
               T.Total_IB_Approved - coalesce(T.Total_IB_MVS_Approved, 0) as Total_IB_FTS_Approved, 

               T.Total_IB_MVS,
               T.Total_IB_MVS_Approved,

               T.Total_KC_MQ_IB_FTS,
               T.Total_KC_MQ_IB_FTS_Approved,

               T.Total_KC_MQ_IB_MVS,
               T.Total_KC_MQ_IB_MVS_Approved

         FROM (
            select fee.Country, 
                   sum(fee.IB) as Total_IB, 
                   sum(fee.IB_Approved) as Total_IB_Approved,
                    
                   sum(fee.IsMultiVendor * fee.IB) as Total_IB_MVS,
                   sum(fee.IsMultiVendor * fee.IB_Approved) as Total_IB_MVS_Approved,

                   sum(case when fee.MaxQty = 0 then 0 else fee.IsMultiVendor * fee.CostPerKit / fee.MaxQty * fee.IB end) as Total_KC_MQ_IB_MVS,
                   sum(case when fee.MaxQty = 0 then 0 else fee.IsMultiVendor * fee.CostPerKit_Approved / fee.MaxQty * fee.IB_Approved end) as Total_KC_MQ_IB_MVS_Approved,

                   sum(case when fee.MaxQty = 0 then 0 else (1 - fee.IsMultiVendor) * fee.CostPerKit / fee.MaxQty * fee.IB end) as Total_KC_MQ_IB_FTS,
                   sum(case when fee.MaxQty = 0 then 0 else (1 - fee.IsMultiVendor) * fee.CostPerKit_Approved / fee.MaxQty * fee.IB_Approved end) as Total_KC_MQ_IB_FTS_Approved

            from Hardware.AvailabilityFeeVIEW fee
            group by fee.Country 
        ) T
    )
    , AvFeeCte as (
        select fee.*,

               ib.Total_IB,
               ib.Total_IB_Approved,

               (case fee.IsMultiVendor 
                    when 1 then ib.Total_IB_MVS
                    else ib.Total_IB_FTS
                end) as Total_IB_VENDOR,               

               (case fee.IsMultiVendor 
                    when 1 then ib.Total_IB_MVS_Approved
                    else ib.Total_IB_FTS_Approved
                end) as Total_IB_VENDOR_Approved,               

               (case fee.IsMultiVendor
                    when 1 then ib.Total_KC_MQ_IB_MVS
                    else ib.Total_KC_MQ_IB_FTS
                end) as Total_KC_MQ_IB_VENDOR,

               (case fee.IsMultiVendor
                    when 1 then ib.Total_KC_MQ_IB_MVS_Approved
                    else ib.Total_KC_MQ_IB_FTS_Approved
                end) as Total_KC_MQ_IB_VENDOR_Approved

        from Hardware.AvailabilityFeeVIEW fee
        join InstallByCountryCte ib on ib.CountryID = fee.Country
    )
    , AvFeeCte2 as (
        select fee.*,

               Hardware.CalcYI(fee.StockValue, fee.AverageContractDuration) as YI,
               Hardware.CalcYI(fee.StockValue_Approved, fee.AverageContractDuration_Approved) as YI_Approved,
          
               Hardware.CalcTISC(fee.TotalLogisticsInfrastructureCost, fee.Total_IB, fee.Total_IB_VENDOR) as TISC,
               Hardware.CalcTISC(fee.TotalLogisticsInfrastructureCost_Approved, fee.Total_IB_Approved, fee.Total_IB_VENDOR_Approved) as TISC_Approved

        from AvFeeCte fee
    )
    select fee.*, 
           case when IB > 0 and fee.MaxQty > 0 then Hardware.CalcAvailabilityFee(fee.CostPerKit, fee.MaxQty, fee.TISC, fee.YI, fee.Total_KC_MQ_IB_VENDOR) else 0 end as Fee,
           case when IB_Approved > 0 and fee.MaxQty > 0 then Hardware.CalcAvailabilityFee(fee.CostPerKit_Approved, fee.MaxQty, fee.TISC_Approved, fee.YI_Approved, fee.Total_KC_MQ_IB_VENDOR_Approved) else 0 end as Fee_Approved
    from AvFeeCte2 fee

GO

exec Hardware.UpdateAvailabilityFee;
go


