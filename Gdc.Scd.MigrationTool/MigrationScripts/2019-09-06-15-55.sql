if OBJECT_ID('dbo.spDropTable') is not null
    drop procedure dbo.spDropTable;
go

create procedure dbo.spDropTable(
    @tableName NVARCHAR(128)
)
as
begin

    if OBJECT_ID(@tableName) is null
        return;

    declare @sql nvarchar(255) = N'DROP TABLE ' + @tableName;
    EXEC sp_executesql @sql;

end
go

if OBJECT_ID('dbo.spDropIndex') is not null
    drop procedure dbo.spDropIndex;
go

create procedure dbo.spDropIndex(
    @tableName NVARCHAR(128),
    @indexName NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @indexName = REPLACE(REPLACE(@indexName, '[', ''), ']', '');

    if not exists(SELECT *
                FROM sys.indexes i
                WHERE i.object_id = OBJECT_ID(@tableName)
                AND i.name = @indexName)
        return;

    declare @sql nvarchar(255) = N'DROP INDEX ' + @indexName + ' ON ' + @tableName;
    EXEC sp_executesql @sql;

end
go

if OBJECT_ID('dbo.spDropColumn') is not null
    drop procedure dbo.spDropColumn;
go

create procedure dbo.spDropColumn(
    @tableName NVARCHAR(128),
    @colName   NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @colName = REPLACE(REPLACE(@colName, '[', ''), ']', '');

    if not exists(SELECT 1 FROM sys.columns WHERE Name = @colName AND Object_ID = Object_ID(@tableName))
        return;

    declare @sql nvarchar(255) = N'alter table ' + @tableName + ' drop column ' + @colName;
    EXEC sp_executesql @sql;

end
go

if OBJECT_ID('dbo.spDropConstaint') is not null
    drop procedure dbo.spDropConstaint;
go

create procedure dbo.spDropConstaint(
    @tableName    NVARCHAR(128),
    @constraint   NVARCHAR(128)
)
as
begin

    set @tableName = REPLACE(REPLACE(@tableName, '[', ''), ']', '');
    set @constraint = REPLACE(REPLACE(@constraint, '[', ''), ']', '');

    IF NOT EXISTS (select * from INFORMATION_SCHEMA.TABLE_CONSTRAINTS where CONSTRAINT_NAME=@constraint)
        return;

    declare @sql nvarchar(255) = N'alter table ' + @tableName + ' DROP CONSTRAINT ' + @constraint;
    EXEC sp_executesql @sql;

end
go

exec spDropConstaint '[Hardware].[MarkupOtherCosts]', '[PK_Hardware_MarkupOtherCosts_Id]';
exec spDropConstaint '[Hardware].[MarkupOtherCosts]', '[PK_Hardware_MarkupOtherCosts]';

exec spDropIndex '[Hardware].[MarkupOtherCosts]', '[ix_Atom_MarkupOtherCosts]';
exec spDropIndex '[Hardware].[MarkupOtherCosts]', '[IX_Hardware_MarkupOtherCosts_Country]';

GO

exec spDropColumn 'Hardware.MarkupOtherCosts', 'MarkupFactor_norm';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'MarkupFactor_norm_Approved';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'ProlongationMarkupFactor_norm';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'ProlongationMarkupFactor_norm_Approved';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'ProlongationMarkup';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'ProlongationMarkup_Approved';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'ProlongationMarkupFactor';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'ProlongationMarkupFactor_Approved';
exec spDropColumn 'Hardware.MarkupOtherCosts', 'Deactivated';

exec spDropColumn 'History.Hardware_MarkupOtherCosts', 'ProlongationMarkup';
exec spDropColumn 'History.Hardware_MarkupOtherCosts', 'ProlongationMarkupFactor';

go

alter table Hardware.MarkupOtherCosts
    add
      [ProlongationMarkup]                float    
    , [ProlongationMarkup_Approved]       float
    , [ProlongationMarkupFactor]          float
    , [ProlongationMarkupFactor_Approved] float

    , [MarkupFactor_norm]  AS ([MarkupFactor]/(100)) PERSISTED
    , [MarkupFactor_norm_Approved]  AS ([MarkupFactor_Approved]/(100)) PERSISTED
    , [ProlongationMarkupFactor_norm]  AS ([ProlongationMarkupFactor]/(100)) PERSISTED
    , [ProlongationMarkupFactor_norm_Approved]  AS ([ProlongationMarkupFactor_Approved]/(100)) PERSISTED

    , Deactivated as cast(case when DeactivatedDateTime is null then 0 else 1 end as bit) PERSISTED not null;
go

alter table [History].[Hardware_MarkupOtherCosts]
    add
      [ProlongationMarkup]                float    
    , [ProlongationMarkupFactor]          float
go

update moc 
    set   ProlongationMarkup = pm.ProlongationMarkup
        , ProlongationMarkup_Approved = pm.ProlongationMarkup_Approved
        , ProlongationMarkupFactor = pm.ProlongationMarkupFactor
        , ProlongationMarkupFactor_Approved = pm.ProlongationMarkupFactor_Approved
from Hardware.MarkupOtherCosts moc
JOIN Hardware.ProlongationMarkup pm on pm.Wg = moc.Wg
                                        AND pm.Country = moc.Country
                                        AND pm.ReactionTimeTypeAvailability = moc.ReactionTimeTypeAvailability 
go

insert into History.Hardware_MarkupOtherCosts(
                  Country
                , Pla
                , Wg
                , ReactionTimeTypeAvailability
                , CentralContractGroup
                , CostBlockHistory
                , ProlongationMarkup
                , ProlongationMarkupFactor
            )
select    Country
        , Pla
        , Wg
        , ReactionTimeTypeAvailability
        , CentralContractGroup
        , CostBlockHistory
        , ProlongationMarkup
        , ProlongationMarkupFactor
from History.Hardware_ProlongationMarkup;
go

update History.CostBlockHistory set Context_CostBlockId = 'MarkupOtherCosts' where Context_CostBlockId = 'ProlongationMarkup';
go

ALTER TABLE [Hardware].[MarkupOtherCosts] ADD CONSTRAINT [PK_Hardware_MarkupOtherCosts] PRIMARY KEY CLUSTERED 
(
    [Country] ASC,
    [Wg] ASC,
    [ReactionTimeTypeAvailability] ASC,
    [Deactivated] asc
)
GO

ALTER FUNCTION [Hardware].[GetCalcMember] (
    @approved       bit,
    @cnt            dbo.ListID readonly,
    @wg             dbo.ListID readonly,
    @av             dbo.ListID readonly,
    @dur            dbo.ListID readonly,
    @reactiontime   dbo.ListID readonly,
    @reactiontype   dbo.ListID readonly,
    @loc            dbo.ListID readonly,
    @pro            dbo.ListID readonly,
    @lastid         bigint,
    @limit          int
)
RETURNS TABLE 
AS
RETURN 
(
    SELECT    m.Id

            --SLA

            , m.CountryId          
            , std.Country
            , std.CurrencyId
            , std.Currency
            , std.ExchangeRate
            , m.WgId
            , std.Wg
            , std.SogId
            , std.Sog
            , m.DurationId
            , dur.Name             as Duration
            , dur.Value            as Year
            , dur.IsProlongation   as IsProlongation
            , m.AvailabilityId
            , av.Name              as Availability
            , m.ReactionTimeId
            , rtime.Name           as ReactionTime
            , m.ReactionTypeId
            , rtype.Name           as ReactionType
            , m.ServiceLocationId
            , loc.Name             as ServiceLocation
            , m.ProActiveSlaId
            , prosla.ExternalName  as ProActiveSla

            , m.Sla
            , m.SlaHash

            , std.StdWarranty
            , std.StdWarrantyLocation

            --Cost values

            , std.AFR1  
            , std.AFR2  
            , std.AFR3  
            , std.AFR4  
            , std.AFR5  
            , std.AFRP1 

            , std.MatCost1
            , std.MatCost2
            , std.MatCost3
            , std.MatCost4
            , std.MatCost5
            , std.MatCost1P

            , std.MatOow1 
            , std.MatOow2 
            , std.MatOow3 
            , std.MatOow4 
            , std.MatOow5 
            , std.MatOow1p

            , std.MaterialW

            , std.TaxAndDuties1
            , std.TaxAndDuties2
            , std.TaxAndDuties3
            , std.TaxAndDuties4
            , std.TaxAndDuties5
            , std.TaxAndDuties1P

            , std.TaxOow1 
            , std.TaxOow2 
            , std.TaxOow3 
            , std.TaxOow4 
            , std.TaxOow5 
            , std.TaxOow1P
            
            , std.TaxAndDutiesW

            , r.Cost as Reinsurance

            --##### FIELD SERVICE COST #########                                                                                               
            , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved                 end / std.ExchangeRate as LabourCost             
            , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved                 end / std.ExchangeRate as TravelCost             
            , case when @approved = 0 then fst.PerformanceRate                else fst.PerformanceRate_Approved            end / std.ExchangeRate as PerformanceRate        
            , case when @approved = 0 then fst.TimeAndMaterialShare_norm      else fst.TimeAndMaterialShare_norm_Approved  end as TimeAndMaterialShare   
            , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved                 end as TravelTime             
            , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved                 end as RepairTime             

            , std.OnsiteHourlyRates      
                       
            --##### SERVICE SUPPORT COST #########                                                                                               
            , std.ServiceSupportPerYear

            --##### LOGISTICS COST #########                                                                                               
            , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved             end / std.ExchangeRate as ExpressDelivery         
            , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved    end / std.ExchangeRate as HighAvailabilityHandling
            , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved            end / std.ExchangeRate as StandardDelivery        
            , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved            end / std.ExchangeRate as StandardHandling        
            , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved       end / std.ExchangeRate as ReturnDeliveryFactory   
            , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved         end / std.ExchangeRate as TaxiCourierDelivery     
                                                                                                                       
            , case when afEx.id is not null then std.Fee else 0 end as AvailabilityFee

            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.Markup else moc.ProlongationMarkup end)                             
                    else (case when dur.IsProlongation = 0 then moc.Markup_Approved else moc.ProlongationMarkup_Approved end)                      
                end / std.ExchangeRate as MarkupOtherCost                      
            , case when @approved = 0 
                    then (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm else moc.ProlongationMarkupFactor_norm end)                             
                    else (case when dur.IsProlongation = 0 then moc.MarkupFactor_norm_Approved else moc.ProlongationMarkupFactor_norm_Approved end)                      
                end as MarkupFactorOtherCost                

            --####### PROACTIVE COST ###################
            , std.LocalRemoteAccessSetup
            , std.LocalRegularUpdate * proSla.LocalRegularUpdateReadyRepetition                 as LocalRegularUpdate
            , std.LocalPreparation * proSla.LocalPreparationShcRepetition                       as LocalPreparation
            , std.LocalRemoteCustomerBriefing * proSla.LocalRemoteShcCustomerBriefingRepetition as LocalRemoteCustomerBriefing
            , std.LocalOnsiteCustomerBriefing * proSla.LocalOnsiteShcCustomerBriefingRepetition as LocalOnsiteCustomerBriefing
            , std.Travel * proSla.TravellingTimeRepetition                                      as Travel
            , std.CentralExecutionReport * proSla.CentralExecutionShcReportRepetition           as CentralExecutionReport

            , std.LocalServiceStandardWarranty
            , std.LocalServiceStandardWarrantyManual
            , std.Credit1
            , std.Credit2
            , std.Credit3
            , std.Credit4
            , std.Credit5
            , std.Credits

            --########## MANUAL COSTS ################
            , man.ListPrice          / std.ExchangeRate as ListPrice                   
            , man.DealerDiscount                        as DealerDiscount              
            , man.DealerPrice        / std.ExchangeRate as DealerPrice                 
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTC     / std.ExchangeRate) end as ServiceTCManual                   
            , case when std.CanOverrideTransferCostAndPrice = 1 then (man.ServiceTP     / std.ExchangeRate) end as ServiceTPManual                   
            , man.ServiceTP_Released / std.ExchangeRate as ServiceTP_Released                  
            , man.ReleaseDate                           as ReleaseDate
            , man.ChangeDate                            
            , u.Name                                    as ChangeUserName
            , u.Email                                   as ChangeUserEmail

    FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    INNER JOIN Hardware.CalcStdw(@approved, @cnt, @wg) std on std.CountryId = m.CountryId and std.WgId = m.WgId

    LEFT JOIN Hardware.GetReinsurance(@approved) r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.ReactionTimeAvailability = m.ReactionTime_Avalability

    LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
    LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Country = m.CountryId AND fst.Wg = m.WgId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

    LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTimeType = m.ReactionTime_ReactionType and lc.DeactivatedDateTime is null

    LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Country = m.CountryId AND moc.Wg = m.WgId AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability and moc.Deactivated = 0

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ManualCost man on man.PortfolioId = m.Id

    LEFT JOIN dbo.[User] u on u.Id = man.ChangeUserId
)
go

alter function [Report].[GetParameterHw]
(
    @approved bit,
    @cnt bigint,
    @wg bigint,
    @av bigint,
    @duration     bigint,
    @reactiontime bigint,
    @reactiontype bigint,
    @loc bigint,
    @pro bigint
)
RETURNS TABLE 
AS
RETURN (
    with CountryCte as (
        select c.*
             , cur.Name as Currency
             , er.Value as ExchangeRate
             , case when @approved = 0 then tax.TaxAndDuties else tax.TaxAndDuties_Approved end as TaxAndDuties
        from InputAtoms.Country c 
        INNER JOIN [References].Currency cur on cur.Id = c.CurrencyId
        INNER JOIN [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        LEFT JOIN Hardware.TaxAndDuties tax on tax.Country = c.Id and tax.DeactivatedDateTime is null
        where c.Id = @cnt
    )
    , WgCte as (
        select wg.Id
             , wg.Name
             , wg.Description
             , wg.SCD_ServiceType
             , pla.ClusterPlaId
             , sog.Description as SogDescription
             , wg.RoleCodeId
        
             , case when @approved = 0 then afr.AFR1                           else AFR1_Approved                               end as AFR1 
             , case when @approved = 0 then afr.AFR2                           else AFR2_Approved                               end as AFR2 
             , case when @approved = 0 then afr.AFR3                           else afr.AFR3_Approved                           end as AFR3 
             , case when @approved = 0 then afr.AFR4                           else afr.AFR4_Approved                           end as AFR4 
             , case when @approved = 0 then afr.AFR5                           else afr.AFR5_Approved                           end as AFR5 
             , case when @approved = 0 then afr.AFRP1                          else afr.AFRP1_Approved                          end as AFRP1

             , r.ReinsuranceFlatfee1              
             , r.ReinsuranceFlatfee2              
             , r.ReinsuranceFlatfee3              
             , r.ReinsuranceFlatfee4              
             , r.ReinsuranceFlatfee5              
             , r.ReinsuranceFlatfeeP1             
             , r.ReinsuranceUpliftFactor_4h_24x7  
             , r.ReinsuranceUpliftFactor_4h_9x5   
             , r.ReinsuranceUpliftFactor_NBD_9x5  

        from InputAtoms.Wg wg 
        INNER JOIN InputAtoms.Pla pla on pla.id = wg.PlaId
        LEFT JOIN InputAtoms.Sog sog on sog.id = wg.SogId
        LEFT JOIN Hardware.AfrYear afr on afr.Wg = wg.Id
        LEFT JOIN Report.GetReinsuranceYear(@approved) r on r.Wg = wg.Id
        where wg.DeactivatedDateTime is null and (@wg is null or wg.Id = @wg)
    )
    , CostCte as (
            select 
                m.Id
                , m.CountryId
                , c.Name as Country
                , wg.Description as WgDescription
                , wg.Name as Wg
                , wg.SogDescription
                , wg.SCD_ServiceType
                , pro.ExternalName as Sla
                , loc.Name as ServiceLocation
                , rtime.Name as ReactionTime
                , rtype.Name as ReactionType
                , av.Name as Availability
                , c.Currency
                , c.ExchangeRate

                --FSP
                , fsp.Name Fsp
                , fsp.ServiceDescription as FspDescription

                --cost blocks

                , case when @approved = 0 then fsc.LabourCost else fsc.LabourCost_Approved end as LabourCost
                , case when @approved = 0 then fsc.TravelCost else fsc.TravelCost_Approved end as TravelCost
                , case when @approved = 0 then fst.PerformanceRate else fst.PerformanceRate_Approved end as PerformanceRate
                , case when @approved = 0 then fsc.TravelTime else fsc.TravelTime_Approved end as TravelTime
                , case when @approved = 0 then fsc.RepairTime else fsc.RepairTime_Approved end as RepairTime
                , case when @approved = 0 then hr.OnsiteHourlyRates else hr.OnsiteHourlyRates_Approved end as OnsiteHourlyRate


                , case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end         as StandardHandling
                , case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling 
                , case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end         as StandardDelivery
                , case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end          as ExpressDelivery
                , case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end      as TaxiCourierDelivery
                , case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end    as ReturnDeliveryFactory 
                , case when @approved = 0 then lc.StandardHandling else lc.StandardHandling_Approved end + case when @approved = 0 then lc.HighAvailabilityHandling else lc.HighAvailabilityHandling_Approved end as LogisticHandlingPerYear
                , case when @approved = 0 then lc.StandardDelivery else lc.StandardDelivery_Approved end + case when @approved = 0 then lc.ExpressDelivery else lc.ExpressDelivery_Approved end + case when @approved = 0 then lc.TaxiCourierDelivery else lc.TaxiCourierDelivery_Approved end + case when @approved = 0 then lc.ReturnDeliveryFactory else lc.ReturnDeliveryFactory_Approved end as LogisticTransportPerYear

                , case when afEx.id is not null then case when @approved = 0 then af.Fee else af.Fee_Approved end else 0 end as AvailabilityFee
      
                , c.TaxAndDuties as TaxAndDutiesW

                , case when @approved = 0 then moc.Markup else moc.Markup_Approved end       as MarkupOtherCost
                , case when @approved = 0 then moc.MarkupFactor else moc.MarkupFactor_Approved end as MarkupFactorOtherCost

                , case when @approved = 0 then msw.MarkupFactorStandardWarranty else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty
                , case when @approved = 0 then msw.MarkupStandardWarranty else msw.MarkupStandardWarranty_Approved end       as MarkupStandardWarranty
      
                , wg.AFR1
                , wg.AFR2
                , wg.AFR3
                , wg.AFR4
                , wg.AFR5
                , wg.AFRP1

                , Hardware.CalcFieldServiceCost(
                            case when @approved = 0 then fst.TimeAndMaterialShare_norm else fst.TimeAndMaterialShare_norm_Approved end, 
                            case when @approved = 0 then fsc.TravelCost                else fsc.TravelCost_Approved end, 
                            case when @approved = 0 then fsc.LabourCost                else fsc.LabourCost_Approved end, 
                            case when @approved = 0 then fst.PerformanceRate           else fst.PerformanceRate_Approved end, 
                            case when @approved = 0 then fsc.TravelTime                else fsc.TravelTime_Approved end, 
                            case when @approved = 0 then fsc.RepairTime                else fsc.RepairTime_Approved end, 
                            case when @approved = 0 then hr.OnsiteHourlyRates          else hr.OnsiteHourlyRates_Approved end, 
                            1
                        ) as FieldServicePerYear

                , case when @approved = 0 then ssc.[1stLevelSupportCosts] else ssc.[1stLevelSupportCosts_Approved] end as [1stLevelSupportCosts]
                , case when @approved = 0 then ssc.[2ndLevelSupportCosts] else ssc.[2ndLevelSupportCosts_Approved] end as [2ndLevelSupportCosts]
           
                , wg.ReinsuranceFlatfee1
                , wg.ReinsuranceFlatfee2
                , wg.ReinsuranceFlatfee3
                , wg.ReinsuranceFlatfee4
                , wg.ReinsuranceFlatfee5
                , wg.ReinsuranceFlatfeeP1
                , wg.ReinsuranceUpliftFactor_4h_24x7
                , wg.ReinsuranceUpliftFactor_4h_9x5
                , wg.ReinsuranceUpliftFactor_NBD_9x5

                , case when @approved = 0 then mcw.MaterialCostIw else mcw.MaterialCostIw_Approved end as MaterialCostWarranty
                , case when @approved = 0 then mcw.MaterialCostOow else mcw.MaterialCostOow_Approved end as MaterialCostOow

                , dur.Value as Duration
                , dur.IsProlongation

        from Portfolio.GetBySlaSingle(@cnt, @wg, @av, @duration, @reactiontime, @reactiontype, @loc, @pro) m

        INNER JOIN CountryCte c on c.Id = m.CountryId

        INNER JOIN WgCte wg on wg.Id = m.WgId

        INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId and dur.IsProlongation = 0

        INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

        INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

        INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId

        INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

        INNER JOIN Dependencies.ProActiveSla pro on pro.Id = m.ProActiveSlaId

        LEFT JOIN Hardware.RoleCodeHourlyRates hr on hr.Country = m.CountryId and hr.RoleCode = wg.RoleCodeId 

        --cost blocks
        LEFT JOIN Hardware.FieldServiceCalc fsc ON fsc.Country = m.CountryId AND fsc.Wg = m.WgId AND fsc.ServiceLocation = m.ServiceLocationId
        LEFT JOIN Hardware.FieldServiceTimeCalc fst ON fst.Wg = m.WgId AND fst.Country = m.CountryId AND fst.ReactionTimeType = m.ReactionTime_ReactionType

        LEFT JOIN Hardware.LogisticsCosts lc on lc.Country = m.CountryId 
                                            AND lc.Wg = m.WgId
                                            AND lc.ReactionTimeType = m.ReactionTime_ReactionType
                                            AND lc.DeactivatedDateTime is null

        LEFT JOIN Hardware.MaterialCostWarrantyCalc mcw on mcw.Country = m.CountryId and mcw.Wg = m.WgId 

        LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPlaId

        LEFT JOIN Hardware.MarkupOtherCosts moc on moc.Wg = m.WgId 
                                               AND moc.Country = m.CountryId 
                                               AND moc.ReactionTimeTypeAvailability = m.ReactionTime_ReactionType_Avalability 
                                               and moc.Deactivated = 0

        LEFT JOIN Hardware.MarkupStandardWaranty msw on msw.Country = m.CountryId AND msw.Wg = m.WgId and msw.DeactivatedDateTime is null

        LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

        LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId 
                                            AND afEx.ReactionTimeId = m.ReactionTimeId 
                                            AND afEx.ReactionTypeId = m.ReactionTypeId 
                                            AND afEx.ServiceLocationId = m.ServiceLocationId

        LEFT JOIN Fsp.HwFspCodeTranslation fsp  on fsp.SlaHash = m.SlaHash and fsp.Sla = m.Sla
    )
    select    
                m.Id
              , m.Country
              , m.WgDescription
              , m.Wg
              , m.SogDescription
              , m.SCD_ServiceType
              , m.Sla
              , m.ServiceLocation
              , m.ReactionTime
              , m.ReactionType
              , m.Availability

             , m.Currency

             --FSP
              , m.Fsp
              , m.FspDescription

              --cost blocks

              , m.LabourCost as LabourCost
              , m.TravelCost as TravelCost
              , m.PerformanceRate as PerformanceRate
              , m.TravelTime
              , m.RepairTime
              , m.OnsiteHourlyRate as OnsiteHourlyRate

              , m.AvailabilityFee * m.ExchangeRate as AvailabilityFee
      
              , m.TaxAndDutiesW as TaxAndDutiesW

              , m.MarkupOtherCost as MarkupOtherCost
              , m.MarkupFactorOtherCost as MarkupFactorOtherCost

              , m.MarkupFactorStandardWarranty as MarkupFactorStandardWarranty
              , m.MarkupStandardWarranty as MarkupStandardWarranty
      
              , m.AFR1   * 100 as AFR1
              , m.AFR2   * 100 as AFR2
              , m.AFR3   * 100 as AFR3
              , m.AFR4   * 100 as AFR4
              , m.AFR5   * 100 as AFR5
              , m.AFRP1  * 100 as AFRP1

              , m.[1stLevelSupportCosts] * m.ExchangeRate as [1stLevelSupportCosts]
              , m.[2ndLevelSupportCosts] * m.ExchangeRate as [2ndLevelSupportCosts]
           
              , m.ReinsuranceFlatfee1 * m.ExchangeRate as ReinsuranceFlatfee1
              , m.ReinsuranceFlatfee2 * m.ExchangeRate as ReinsuranceFlatfee2
              , m.ReinsuranceFlatfee3 * m.ExchangeRate as ReinsuranceFlatfee3
              , m.ReinsuranceFlatfee4 * m.ExchangeRate as ReinsuranceFlatfee4
              , m.ReinsuranceFlatfee5 * m.ExchangeRate as ReinsuranceFlatfee5
              , m.ReinsuranceFlatfeeP1 * m.ExchangeRate as ReinsuranceFlatfeeP1
              , m.ReinsuranceUpliftFactor_4h_24x7 as ReinsuranceUpliftFactor_4h_24x7
              , m.ReinsuranceUpliftFactor_4h_9x5 as ReinsuranceUpliftFactor_4h_9x5
              , m.ReinsuranceUpliftFactor_NBD_9x5 as ReinsuranceUpliftFactor_NBD_9x5

              , m.MaterialCostWarranty * m.ExchangeRate as MaterialCostWarranty
              , m.MaterialCostOow * m.ExchangeRate as MaterialCostOow

              , m.Duration

              , m.FieldServicePerYear * m.AFR1 as FieldServiceCost1
              , m.FieldServicePerYear * m.AFR2 as FieldServiceCost2
              , m.FieldServicePerYear * m.AFR3 as FieldServiceCost3
              , m.FieldServicePerYear * m.AFR4 as FieldServiceCost4
              , m.FieldServicePerYear * m.AFR5 as FieldServiceCost5
            
              , m.StandardHandling
              , m.HighAvailabilityHandling
              , m.StandardDelivery
              , m.ExpressDelivery
              , m.TaxiCourierDelivery
              , m.ReturnDeliveryFactory 

              , Hardware.CalcByDur(
                      m.Duration
                    , m.IsProlongation 
                    , m.LogisticHandlingPerYear * m.AFR1 
                    , m.LogisticHandlingPerYear * m.AFR2 
                    , m.LogisticHandlingPerYear * m.AFR3 
                    , m.LogisticHandlingPerYear * m.AFR4 
                    , m.LogisticHandlingPerYear * m.AFR5 
                    , m.LogisticHandlingPerYear * m.AFRP1
                ) as LogisticsHandling

             , Hardware.CalcByDur(
                       m.Duration
                     , m.IsProlongation 
                     , m.LogisticTransportPerYear * m.AFR1 
                     , m.LogisticTransportPerYear * m.AFR2 
                     , m.LogisticTransportPerYear * m.AFR3 
                     , m.LogisticTransportPerYear * m.AFR4 
                     , m.LogisticTransportPerYear * m.AFR5 
                     , m.LogisticTransportPerYear * m.AFRP1
                 ) as LogisticTransportcost

    from CostCte m
)
go

sp_rename 'Hardware.ProlongationMarkup','ProlongationMarkup_Backup';
go