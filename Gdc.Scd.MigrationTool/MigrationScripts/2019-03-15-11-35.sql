ALTER PROCEDURE [Hardware].[SpGetCosts]
    @approved     bit,
    @local        bit,
    @cnt          dbo.ListID readonly,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @lastid       bigint,
    @limit        int,
    @total        int output
AS
BEGIN

    SET NOCOUNT ON;

    select @total = COUNT(Id) from Portfolio.GetBySla(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro);

    declare @sla Portfolio.Sla;
    insert into @sla select * from Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    declare @cur nvarchar(max);
    declare @exchange float;

    if @local = 1
    begin
    
        --convert values from EUR to local

        select costs.Id

             , Country
             , cur.Name as Currency
             , costs.ExchangeRate

             , sog.Name as Sog
             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty
             , StdWarrantyLocation

             --Cost

             , AvailabilityFee               * costs.ExchangeRate  as AvailabilityFee 
             , TaxAndDutiesW                 * costs.ExchangeRate  as TaxAndDutiesW
             , TaxAndDutiesOow               * costs.ExchangeRate  as TaxAndDutiesOow
             , Reinsurance                   * costs.ExchangeRate  as Reinsurance
             , ProActive                     * costs.ExchangeRate  as ProActive
             , ServiceSupportCost            * costs.ExchangeRate  as ServiceSupportCost

             , MaterialW                     * costs.ExchangeRate  as MaterialW
             , MaterialOow                   * costs.ExchangeRate  as MaterialOow
             , FieldServiceCost              * costs.ExchangeRate  as FieldServiceCost
             , Logistic                      * costs.ExchangeRate  as Logistic
             , OtherDirect                   * costs.ExchangeRate  as OtherDirect
             , LocalServiceStandardWarranty  * costs.ExchangeRate  as LocalServiceStandardWarranty
             , Credits                       * costs.ExchangeRate  as Credits
             , ServiceTC                     * costs.ExchangeRate  as ServiceTC
             , ServiceTP                     * costs.ExchangeRate  as ServiceTP

             , ServiceTCManual               * costs.ExchangeRate  as ServiceTCManual
             , ServiceTPManual               * costs.ExchangeRate  as ServiceTPManual

             , ServiceTP_Released            * costs.ExchangeRate  as ServiceTP_Released

             , ListPrice                     * costs.ExchangeRate  as ListPrice
             , DealerPrice                   * costs.ExchangeRate  as DealerPrice
             , DealerDiscount                                      as DealerDiscount
                                                             
             , ChangeUserName                                      as ChangeUserName
             , ChangeUserEmail                                     as ChangeUserEmail

        from Hardware.GetCostsSla(@approved, @sla) costs
        join [References].Currency cur on cur.Id = costs.CurrencyId
        join InputAtoms.Wg wg on wg.id = costs.WgId
        left join InputAtoms.Sog sog on sog.id = wg.SogId
        order by Id
        
    end
    else
    begin

        select costs.Id

             , Country
             , 'EUR' as Currency
             , costs.ExchangeRate

             , sog.Name as Sog
             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty
             , StdWarrantyLocation

             --Cost

             , AvailabilityFee               
             , TaxAndDutiesW                 
             , TaxAndDutiesOow               
             , Reinsurance                   
             , ProActive                     
             , ServiceSupportCost            

             , MaterialW                     
             , MaterialOow                   
             , FieldServiceCost              
             , Logistic                      
             , OtherDirect                   
             , LocalServiceStandardWarranty  
             , Credits                       
             , ServiceTC                     
             , ServiceTP                     

             , ServiceTCManual               
             , ServiceTPManual               

             , ServiceTP_Released            

             , ListPrice                     
             , DealerPrice                   
             , DealerDiscount                
                                             
             , ChangeUserName                
             , ChangeUserEmail               

        from Hardware.GetCostsSla(@approved, @sla) costs
        join InputAtoms.Wg wg on wg.id = costs.WgId
        left join InputAtoms.Sog sog on sog.id = wg.SogId
        order by Id
    end
END

