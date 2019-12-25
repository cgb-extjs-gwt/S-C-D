IF OBJECT_ID('Hardware.SpGetCosts') IS NOT NULL
  DROP PROCEDURE Hardware.SpGetCosts;
go

CREATE PROCEDURE [Hardware].[SpGetCosts]
    @approved     bit,
    @local        bit,
    @cnt          dbo.ListID readonly,
    @fsp          nvarchar(255),
    @hasFsp       bit,
    @wg           dbo.ListID readonly,
    @av           dbo.ListID readonly,
    @dur          dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc          dbo.ListID readonly,
    @pro          dbo.ListID readonly,
    @lastid       bigint,
    @limit        int
AS
BEGIN

    SET NOCOUNT ON;

    if @local = 1
    begin
    
        --convert values from EUR to local

        select 
               rownum
             , Id

             , Fsp
             , Country
             , Currency
             , ExchangeRate

             , Sog
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

             , AvailabilityFee               * ExchangeRate  as AvailabilityFee 
             , TaxAndDutiesW                 * ExchangeRate  as TaxAndDutiesW
             , TaxAndDutiesOow               * ExchangeRate  as TaxAndDutiesOow
             , Reinsurance                   * ExchangeRate  as Reinsurance
             , ProActive                     * ExchangeRate  as ProActive
             , ServiceSupportCost            * ExchangeRate  as ServiceSupportCost

             , MaterialW                     * ExchangeRate  as MaterialW
             , MaterialOow                   * ExchangeRate  as MaterialOow
             , FieldServiceCost              * ExchangeRate  as FieldServiceCost
             , Logistic                      * ExchangeRate  as Logistic
             , OtherDirect                   * ExchangeRate  as OtherDirect
             , LocalServiceStandardWarranty  * ExchangeRate  as LocalServiceStandardWarranty
             , LocalServiceStandardWarrantyManual  * ExchangeRate  as LocalServiceStandardWarrantyManual
             , Credits                       * ExchangeRate  as Credits
             , ServiceTC                     * ExchangeRate  as ServiceTC
             , ServiceTP                     * ExchangeRate  as ServiceTP

             , ServiceTCManual               * ExchangeRate  as ServiceTCManual
             , ServiceTPManual               * ExchangeRate  as ServiceTPManual

             , ServiceTP_Released            * ExchangeRate  as ServiceTP_Released

             , ListPrice                     * ExchangeRate  as ListPrice
             , DealerPrice                   * ExchangeRate  as DealerPrice
             , DealerDiscount                                as DealerDiscount
                                                       
             , ReleaseDate                                    
             , ReleaseUserName
             , ReleaseUserEmail

             , ChangeDate
             , ChangeUserName                                as ChangeUserName
             , ChangeUserEmail                               as ChangeUserEmail

        from Hardware.GetCosts2(@approved, @cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) 
        order by rownum
        
    end
    else
    begin

        select                
               rownum
             , Id

             , Fsp
             , Country
             , 'EUR' as Currency
             , ExchangeRate

             , Sog
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
             , LocalServiceStandardWarrantyManual
             , Credits                       
             , ServiceTC                     
             , ServiceTP                     

             , ServiceTCManual               
             , ServiceTPManual               

             , ServiceTP_Released            

             , ListPrice                     
             , DealerPrice                   
             , DealerDiscount                
                                             
             , ReleaseDate                                    
             , ReleaseUserName
             , ReleaseUserEmail

             , ChangeDate
             , ChangeUserName                
             , ChangeUserEmail               

        from Hardware.GetCosts2(@approved, @cnt, @fsp, @hasFsp, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) 
        order by rownum
    end
END
GO