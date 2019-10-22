IF OBJECT_ID('SoftwareSolution.GetProActiveCosts') IS NOT NULL
  DROP FUNCTION SoftwareSolution.GetProActiveCosts;
go 


CREATE FUNCTION SoftwareSolution.GetProActiveCosts (
      @approved bit,
	 @cnt dbo.ListID readonly,
     @digit dbo.ListID readonly,
     @av dbo.ListID readonly,
     @year dbo.ListID readonly,
     @lastid bigint,
     @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with ProActiveCte as (
        select    pro.rownum                
                , pro.Id                    
                , pro.Country               
                , pro.Pla                   
                , pro.Sog                   
                , pro.SwDigit               
                , pro.FspId                 
                , pro.Fsp                   
                , pro.FspServiceDescription 
                , pro.AvailabilityId        
                , pro.DurationId            
                , pro.ReactionTimeId        
                , pro.ReactionTypeId        
                , pro.ServiceLocationId     
                , pro.ProactiveSlaId        

                , pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * sla.LocalPreparationShcRepetition as LocalPreparation

                , pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * sla.LocalRegularUpdateReadyRepetition as LocalRegularUpdate

                , pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalRemoteShcCustomerBriefingRepetition as LocalRemoteCustomerBriefing
                
                , pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * sla.LocalOnsiteShcCustomerBriefingRepetition as LocalOnsiteCustomerBriefing
                
                , pro.TravellingTime * pro.OnSiteHourlyRate * sla.TravellingTimeRepetition as Travel

                , pro.CentralExecutionShcReportCost * sla.CentralExecutionShcReportRepetition as CentralExecutionReport

                , pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate as Setup

        FROM SoftwareSolution.GetProActivePaging(@approved, @cnt, @digit, @av, @year, @lastid, @limit) pro
        LEFT JOIN Dependencies.ProActiveSla sla on sla.id = pro.ProactiveSlaId
    )
    , ProActiveCte2 as (
         select pro.*

               , pro.LocalPreparation + 
                 pro.LocalRegularUpdate + 
                 pro.LocalRemoteCustomerBriefing +
                 pro.LocalOnsiteCustomerBriefing +
                 pro.Travel +
                 pro.CentralExecutionReport as Service

        from ProActiveCte pro
    )
    select pro.*
         , Hardware.CalcProActive(pro.Setup, pro.Service, dur.Value) as ProActive
    from ProActiveCte2 pro
    LEFT JOIN Dependencies.Duration dur on dur.Id = pro.DurationId
);
GO