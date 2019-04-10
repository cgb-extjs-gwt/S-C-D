if OBJECT_ID('Archive.spGetHwCosts') is not null
    drop procedure Archive.spGetHwCosts;
go

create procedure Archive.spGetHwCosts(
    @cnt bigint 
)
AS
begin

    declare @cntTbl       dbo.ListID ;
    declare @wg           dbo.ListID ;
    declare @av           dbo.ListID ;
    declare @dur          dbo.ListID ;
    declare @reactiontime dbo.ListID ;
    declare @reactiontype dbo.ListID ;
    declare @loc          dbo.ListID ;
    declare @pro          dbo.ListID ;

    insert into @cntTbl(id) values (@cnt);

    select Country
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

    from Hardware.GetCosts(1, @cntTbl, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, null, null) 
end
go
   