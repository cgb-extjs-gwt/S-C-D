IF OBJECT_ID('Hardware.HddRetentionManualCost', 'U') IS NOT NULL
  DROP TABLE Hardware.HddRetentionManualCost;
go

CREATE TABLE Hardware.HddRetentionManualCost (
       [WgId] [bigint] NOT NULL primary key foreign key references InputAtoms.Wg(Id)
     , [ChangeUserId] [bigint] NOT NULL foreign key REFERENCES [dbo].[User] ([Id])
     , [TransferPrice] [float] NULL
     , [ListPrice] [float] NULL
     , [DealerDiscount] [float] NULL
     , [DealerPrice]  AS ([ListPrice]-([ListPrice]*[DealerDiscount])/(100))
) 

GO

IF OBJECT_ID('Hardware.HddRetentionView', 'V') IS NOT NULL
  DROP VIEW Hardware.HddRetentionView;
go

CREATE VIEW Hardware.HddRetentionView as 
    SELECT 
           h.Wg as WgId
         , wg.Name as Wg
         , h.HddRet
         , HddRet_Approved
         , hm.TransferPrice 
         , hm.ListPrice
         , hm.DealerDiscount
         , hm.DealerPrice
         , u.Name as ChangeUserName
         , u.Email as ChangeUserEmail

    FROM Hardware.HddRetention h
    JOIN InputAtoms.Wg wg on wg.id = h.Wg
    LEFT JOIN Hardware.HddRetentionManualCost hm on hm.WgId = h.Wg
    LEFT JOIN [dbo].[User] u on u.Id = hm.ChangeUserId
    WHERE h.DeactivatedDateTime is null 
      AND h.Year = (select id from Dependencies.Year where Value = 5 and IsProlongation = 0)
go

insert into Report.Report(Name, Title, CountrySpecific, HasFreesedVersion, SqlFunc) 
 values('HDD-RETENTION-CALC-RESULT', 'Hdd retention service costs', 0, 0, 'Report.HddRetentionCalcResult');

 IF OBJECT_ID('Report.HddRetentionCalcResult') IS NOT NULL
  DROP FUNCTION Report.HddRetentionCalcResult;
go 

CREATE FUNCTION Report.HddRetentionCalcResult()
RETURNS TABLE 
AS
RETURN (
    select h.Wg
         , h.TransferPrice
         , h.ListPrice
         , h.DealerDiscount
         , h.DealerPrice
         , h.ChangeUserName + '[' + h.ChangeUserEmail + ']' as ChangeUser
    from Hardware.HddRetentionView h
)
GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-CALC-RESULT');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'WG(Asset)', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TransferPrice', 'Transfer price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 5, 'DealerDiscount', 'Dealer discount in %', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer price', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'ChangeUser', 'Change user', 1, 1);


GO

IF OBJECT_ID('Report.HddRetentionCentral') IS NOT NULL
  DROP FUNCTION Report.HddRetentionCentral;
go 

CREATE FUNCTION Report.HddRetentionCentral
(
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    select    wg.Name as Wg
            , wg.Description as WgDescription
            , hdd.TransferPrice
            , hdd.DealerPrice as DealerPrice
            , hdd.ListPrice as ListPrice
    from Hardware.HddRetentionView hdd
    join InputAtoms.WgSogView wg on wg.Id = hdd.WgId
    where (@wg is null or wg.Id = @wg)
)

GO

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-CENTRAL');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TransferPrice', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where UPPER(name) = 'WG'), 'wg', 'Warranty Group');

GO

IF OBJECT_ID('Report.HddRetentionByCountry') IS NOT NULL
  DROP FUNCTION Report.HddRetentionByCountry;
go 

CREATE FUNCTION Report.HddRetentionByCountry
(
    @cnt bigint,
    @wg bigint
)
RETURNS TABLE 
AS
RETURN (
    SELECT c.CountryGroup
         , c.Name as Country
         , wg.Name as Wg
         , wg.Description as WgDescription
         , fsp.Name as Fsp
         , fsp.Name as TopFsp

         , hdd.TransferPrice
         , hdd.DealerPrice
         , hdd.ListPrice

    from Fsp.HwHddFspCodeTranslation fsp
    join InputAtoms.CountryView c on c.Id = fsp.CountryId
    join InputAtoms.WgSogView wg on wg.id = fsp.WgId
    left join Hardware.HddRetentionView hdd on hdd.WgId = fsp.WgId

    where     (@cnt is null or fsp.CountryId = @cnt)
          and (@wg is null or fsp.WgId = @wg)
)

go

declare @reportId bigint = (select Id from Report.Report where upper(Name) = 'HDD-RETENTION-COUNTRY');
declare @index int = 0;

delete from Report.ReportColumn where ReportId = @reportId;
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'CountryGroup', 'Country Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Country', 'Country Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Wg', 'Warranty Group', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'WgDescription', 'Warranty Group Name', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'Fsp', 'Support Pack Code', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 1, 'TopFsp', 'TopUp Code', 1, 1);

set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'TransferPrice', 'Transfer Price', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'DealerPrice', 'Dealer Price (Central Reference)', 1, 1);
set @index = @index + 1;
insert into Report.ReportColumn(ReportId, [Index], TypeId, Name, Text, AllowNull, Flex) values(@reportId, @index, 4, 'ListPrice', 'List Price (Central Reference)', 1, 1);

set @index = 0;
delete from Report.ReportFilter where ReportId = @reportId;

set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where UPPER(name) = 'USERCOUNTRY'), 'cnt', 'Country Name');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, (select id from Report.ReportFilterType where UPPER(name) = 'WG'), 'wg', 'Warranty Group');

go

ALTER FUNCTION [Hardware].[GetCalcMember] (
    @approved bit,
	@cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with Cte as (
        SELECT 
                   m.*

                 , case when stdw.DurationValue is not null then stdw.DurationValue 
                        when stdw2.DurationValue is not null then stdw2.DurationValue 
                    end as StdWarranty
                 , case when stdw.DurationValue is not null then stdw.ReactionTimeId 
                        when stdw2.DurationValue is not null then stdw2.ReactionTimeId  
                    end as StdwReactionTimeId 
                 , case when stdw.DurationValue is not null then stdw.ReactionTypeId 
                        when stdw2.DurationValue is not null then stdw2.ReactionTypeId 
                    end as StdwReactionTypeId

        FROM Portfolio.GetBySlaPaging(@cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m

        LEFT JOIN Fsp.HwStandardWarrantyView stdw on stdw.Wg = m.WgId and stdw.Country = m.CountryId --find local standard warranty portfolio
        LEFT JOIN Fsp.HwStandardWarrantyView stdw2 on stdw2.Wg = m.WgId and stdw2.Country is null    --find principle standard warranty portfolio, if local does not exist

    )
    SELECT m.Id

        --SLA

         , m.CountryId          
         , c.Name               as Country
         , m.WgId
         , wg.Name              as Wg
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

         , m.StdWarranty

         , case when @approved = 0 then afr.AFR1  else AFR1_Approved       end as AFR1 
         , case when @approved = 0 then afr.AFR2  else AFR2_Approved       end as AFR2 
         , case when @approved = 0 then afr.AFR3  else afr.AFR3_Approved   end as AFR3 
         , case when @approved = 0 then afr.AFR4  else afr.AFR4_Approved   end as AFR4 
         , case when @approved = 0 then afr.AFR5  else afr.AFR5_Approved   end as AFR5 
         , case when @approved = 0 then afr.AFRP1 else afr.AFRP1_Approved  end as AFRP1
       
         , case when @approved = 0 then mcw.MaterialCostWarranty           else mcw.MaterialCostWarranty_Approved    end as MaterialCostWarranty
         , case when @approved = 0 then mco.MaterialCostOow                else mco.MaterialCostOow_Approved         end as MaterialCostOow     

         , case when @approved = 0 then tax.TaxAndDuties                   else tax.TaxAndDuties_Approved            end as TaxAndDuties

         , case when @approved = 0 then r.Cost                             else r.Cost_Approved                      end as Reinsurance
         , case when @approved = 0 then fsc.LabourCost                     else fsc.LabourCost_Approved              end as LabourCost             
         , case when @approved = 0 then fsc.TravelCost                     else fsc.TravelCost_Approved              end as TravelCost             
         , case when @approved = 0 then fsc.TimeAndMaterialShare           else fsc.TimeAndMaterialShare_Approved    end as TimeAndMaterialShare   
         , case when @approved = 0 then fsc.PerformanceRate                else fsc.PerformanceRate_Approved         end as PerformanceRate        
         , case when @approved = 0 then fsc.TravelTime                     else fsc.TravelTime_Approved              end as TravelTime             
         , case when @approved = 0 then fsc.RepairTime                     else fsc.RepairTime_Approved              end as RepairTime             
         , case when @approved = 0 then fsc.OnsiteHourlyRates              else fsc.OnsiteHourlyRates_Approved       end as OnsiteHourlyRates      
                  
         , case when @approved = 0 then ssc.[1stLevelSupportCosts]         else ssc.[1stLevelSupportCosts_Approved]  end as [1stLevelSupportCosts] 
         , case when @approved = 0 then ssc.[2ndLevelSupportCosts]         else ssc.[2ndLevelSupportCosts_Approved]  end as [2ndLevelSupportCosts] 

         , case when @approved = 0 then ssc.ServiceSupport                 else ssc.ServiceSupport_Approved          end as ServiceSupport
         
         , case when @approved = 0 then lcs.ExpressDelivery                 else lcs.ExpressDelivery_Approved          end as StdExpressDelivery         
         , case when @approved = 0 then lcs.HighAvailabilityHandling        else lcs.HighAvailabilityHandling_Approved end as StdHighAvailabilityHandling
         , case when @approved = 0 then lcs.StandardDelivery                else lcs.StandardDelivery_Approved         end as StdStandardDelivery        
         , case when @approved = 0 then lcs.StandardHandling                else lcs.StandardHandling_Approved         end as StdStandardHandling        
         , case when @approved = 0 then lcs.ReturnDeliveryFactory           else lcs.ReturnDeliveryFactory_Approved    end as StdReturnDeliveryFactory   
         , case when @approved = 0 then lcs.TaxiCourierDelivery             else lcs.TaxiCourierDelivery_Approved      end as StdTaxiCourierDelivery     

         , case when @approved = 0 then lc.ExpressDelivery                 else lc.ExpressDelivery_Approved          end as ExpressDelivery         
         , case when @approved = 0 then lc.HighAvailabilityHandling        else lc.HighAvailabilityHandling_Approved end as HighAvailabilityHandling
         , case when @approved = 0 then lc.StandardDelivery                else lc.StandardDelivery_Approved         end as StandardDelivery        
         , case when @approved = 0 then lc.StandardHandling                else lc.StandardHandling_Approved         end as StandardHandling        
         , case when @approved = 0 then lc.ReturnDeliveryFactory           else lc.ReturnDeliveryFactory_Approved    end as ReturnDeliveryFactory   
         , case when @approved = 0 then lc.TaxiCourierDelivery             else lc.TaxiCourierDelivery_Approved      end as TaxiCourierDelivery     

         , case when afEx.id is not null then (case when @approved = 0 then af.Fee else af.Fee_Approved end)
                 else 0
           end as AvailabilityFee

         , case when @approved = 0 then moc.Markup                         else moc.Markup_Approved                       end as MarkupOtherCost                      
         , case when @approved = 0 then moc.MarkupFactor                   else moc.MarkupFactor_Approved                 end as MarkupFactorOtherCost                

         , case when @approved = 0 then msw.MarkupFactorStandardWarranty   else msw.MarkupFactorStandardWarranty_Approved end as MarkupFactorStandardWarranty
         , case when @approved = 0 then msw.MarkupStandardWarranty         else msw.MarkupStandardWarranty_Approved       end as MarkupStandardWarranty      

         , case when @approved = 0 then pro.LocalRemoteAccessSetupPreparationEffort * pro.OnSiteHourlyRate
                else pro.LocalRemoteAccessSetupPreparationEffort_Approved * pro.OnSiteHourlyRate_Approved
            end as LocalRemoteAccessSetup

         , case when @approved = 0 then pro.LocalRegularUpdateReadyEffort * pro.OnSiteHourlyRate * prosla.LocalRegularUpdateReadyRepetition 
                else pro.LocalRegularUpdateReadyEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRegularUpdateReadyRepetition 
            end as LocalRegularUpdate

         , case when @approved = 0 then pro.LocalPreparationShcEffort * pro.OnSiteHourlyRate * prosla.LocalPreparationShcRepetition 
                else pro.LocalPreparationShcEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalPreparationShcRepetition 
            end as LocalPreparation

         , case when @approved = 0 then pro.LocalRemoteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalRemoteShcCustomerBriefingRepetition 
                else pro.LocalRemoteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalRemoteShcCustomerBriefingRepetition 
            end as LocalRemoteCustomerBriefing

         , case when @approved = 0 then pro.LocalOnsiteShcCustomerBriefingEffort * pro.OnSiteHourlyRate * prosla.LocalOnsiteShcCustomerBriefingRepetition 
                else pro.LocalOnSiteShcCustomerBriefingEffort_Approved * pro.OnSiteHourlyRate_Approved * prosla.LocalOnsiteShcCustomerBriefingRepetition 
            end as LocalOnsiteCustomerBriefing

         , case when @approved = 0 then pro.TravellingTime * pro.OnSiteHourlyRate * prosla.TravellingTimeRepetition 
                else pro.TravellingTime_Approved * pro.OnSiteHourlyRate_Approved * prosla.TravellingTimeRepetition 
            end as Travel

         , case when @approved = 0 then pro.CentralExecutionShcReportCost * prosla.CentralExecutionShcReportRepetition 
                else pro.CentralExecutionShcReportCost_Approved * prosla.CentralExecutionShcReportRepetition 
            end as CentralExecutionReport

         , man.ListPrice       as ListPrice                   
         , man.DealerDiscount  as DealerDiscount              
         , man.DealerPrice     as DealerPrice                 
         , man.ServiceTC       as ServiceTCManual                   
         , man.ServiceTP       as ServiceTPManual                   
         , man.ServiceTP_Released as ServiceTP_Released                  
         , man.ChangeUserName  as ChangeUserName
         , man.ChangeUserEmail as ChangeUserEmail

         , m.SlaHash

    FROM Cte m

    INNER JOIN InputAtoms.Country c on c.id = m.CountryId

    INNER JOIN InputAtoms.WgView wg on wg.id = m.WgId

    INNER JOIN Dependencies.Availability av on av.Id= m.AvailabilityId

    INNER JOIN Dependencies.Duration dur on dur.id = m.DurationId

    INNER JOIN Dependencies.ReactionTime rtime on rtime.Id = m.ReactionTimeId

    INNER JOIN Dependencies.ReactionType rtype on rtype.Id = m.ReactionTypeId
   
    INNER JOIN Dependencies.ServiceLocation loc on loc.Id = m.ServiceLocationId

    INNER JOIN Dependencies.ProActiveSla prosla on prosla.id = m.ProActiveSlaId

    LEFT JOIN Hardware.AfrYear afr on afr.Wg = m.WgId

    LEFT JOIN Hardware.ServiceSupportCostView ssc on ssc.Country = m.CountryId and ssc.ClusterPla = wg.ClusterPla

    LEFT JOIN Hardware.TaxAndDutiesView tax on tax.Country = m.CountryId

    LEFT JOIN Hardware.MaterialCostWarranty mcw on mcw.Wg = m.WgId AND mcw.ClusterRegion = c.ClusterRegionId

    LEFT JOIN Hardware.MaterialCostOowCalc mco on mco.Wg = m.WgId AND mco.Country = m.CountryId

    LEFT JOIN Hardware.ReinsuranceView r on r.Wg = m.WgId AND r.Duration = m.DurationId AND r.AvailabilityId = m.AvailabilityId AND r.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.FieldServiceCostView fsc ON fsc.Wg = m.WgId AND fsc.Country = m.CountryId AND fsc.ServiceLocation = m.ServiceLocationId AND fsc.ReactionTypeId = m.ReactionTypeId AND fsc.ReactionTimeId = m.ReactionTimeId

    LEFT JOIN Hardware.LogisticsCostView lc on lc.Country = m.CountryId AND lc.Wg = m.WgId AND lc.ReactionTime = m.ReactionTimeId AND lc.ReactionType = m.ReactionTypeId

    LEFT JOIN Hardware.LogisticsCostView lcs on lcs.Country = m.CountryId AND lcs.Wg = m.WgId AND lcs.ReactionTime = m.StdwReactionTimeId AND lcs.ReactionType = m.StdwReactionTypeId

    LEFT JOIN Hardware.MarkupOtherCostsView moc on moc.Wg = m.WgId AND moc.Country = m.CountryId AND moc.ReactionTimeId = m.ReactionTimeId AND moc.ReactionTypeId = m.ReactionTypeId AND moc.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.MarkupStandardWarantyView msw on msw.Wg = m.WgId AND msw.Country = m.CountryId AND msw.ReactionTimeId = m.ReactionTimeId AND msw.ReactionTypeId = m.ReactionTypeId AND msw.AvailabilityId = m.AvailabilityId

    LEFT JOIN Hardware.AvailabilityFeeCalc af on af.Country = m.CountryId AND af.Wg = m.WgId

    LEFT JOIN Admin.AvailabilityFee afEx on afEx.CountryId = m.CountryId AND afEx.ReactionTimeId = m.ReactionTimeId AND afEx.ReactionTypeId = m.ReactionTypeId AND afEx.ServiceLocationId = m.ServiceLocationId

    LEFT JOIN Hardware.ProActive pro ON  pro.Country= m.CountryId and pro.Wg= m.WgId

    LEFT JOIN Hardware.ManualCostView man on man.PortfolioId = m.Id
)

go

ALTER FUNCTION [Hardware].[GetCostsFull](
    @approved bit,
	@cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    with CostCte as (
        select    m.*

                , case when m.TaxAndDuties is null then 0 else m.TaxAndDuties end as TaxAndDutiesOrZero

                , case when m.Reinsurance is null then 0 else m.Reinsurance end as ReinsuranceOrZero

                , case when m.AvailabilityFee is null then 0 else m.AvailabilityFee end as AvailabilityFeeOrZero

                , m.Year * m.ServiceSupport as ServiceSupportCost

                , m.TravelCost + m.LabourCost as FieldServicePerYearStdw

                , (1 - m.TimeAndMaterialShare) * (m.TravelCost + m.LabourCost + m.PerformanceRate) + m.TimeAndMaterialShare * ((m.TravelTime + m.repairTime) * m.OnsiteHourlyRates + m.PerformanceRate) as FieldServicePerYear

                , m.StdStandardHandling + m.StdHighAvailabilityHandling + m.StdStandardDelivery + m.StdExpressDelivery + m.StdTaxiCourierDelivery + m.StdReturnDeliveryFactory as LogisticPerYearStdw

                , m.StandardHandling + m.HighAvailabilityHandling + m.StandardDelivery + m.ExpressDelivery + m.TaxiCourierDelivery + m.ReturnDeliveryFactory as LogisticPerYear

                , m.LocalRemoteAccessSetup + m.Year * (m.LocalPreparation + m.LocalRegularUpdate + m.LocalRemoteCustomerBriefing + m.LocalOnsiteCustomerBriefing + m.Travel + m.CentralExecutionReport) as ProActive
       
        from Hardware.GetCalcMember(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) m
    )
    , CostCte2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.MaterialCostWarranty * m.AFR1 else 0 end as mat1
                , case when m.StdWarranty >= 2 then m.MaterialCostWarranty * m.AFR2 else 0 end as mat2
                , case when m.StdWarranty >= 3 then m.MaterialCostWarranty * m.AFR3 else 0 end as mat3
                , case when m.StdWarranty >= 4 then m.MaterialCostWarranty * m.AFR4 else 0 end as mat4
                , case when m.StdWarranty >= 5 then m.MaterialCostWarranty * m.AFR5 else 0 end as mat5
                , 0  as mat1P

                , case when m.StdWarranty >= 1 then 0 else m.MaterialCostOow * m.AFR1 end as matO1
                , case when m.StdWarranty >= 2 then 0 else m.MaterialCostOow * m.AFR2 end as matO2
                , case when m.StdWarranty >= 3 then 0 else m.MaterialCostOow * m.AFR3 end as matO3
                , case when m.StdWarranty >= 4 then 0 else m.MaterialCostOow * m.AFR4 end as matO4
                , case when m.StdWarranty >= 5 then 0 else m.MaterialCostOow * m.AFR5 end as matO5
                , m.MaterialCostOow * m.AFRP1 as matO1P

                , m.FieldServicePerYearStdw * m.AFR1  as FieldServiceCostStdw1
                , m.FieldServicePerYearStdw * m.AFR2  as FieldServiceCostStdw2
                , m.FieldServicePerYearStdw * m.AFR3  as FieldServiceCostStdw3
                , m.FieldServicePerYearStdw * m.AFR4  as FieldServiceCostStdw4
                , m.FieldServicePerYearStdw * m.AFR5  as FieldServiceCostStdw5

                , m.FieldServicePerYear * m.AFR1  as FieldServiceCost1
                , m.FieldServicePerYear * m.AFR2  as FieldServiceCost2
                , m.FieldServicePerYear * m.AFR3  as FieldServiceCost3
                , m.FieldServicePerYear * m.AFR4  as FieldServiceCost4
                , m.FieldServicePerYear * m.AFR5  as FieldServiceCost5
                , m.FieldServicePerYear * m.AFRP1 as FieldServiceCost1P

                , m.LogisticPerYearStdw * m.AFR1  as LogisticStdw1
                , m.LogisticPerYearStdw * m.AFR2  as LogisticStdw2
                , m.LogisticPerYearStdw * m.AFR3  as LogisticStdw3
                , m.LogisticPerYearStdw * m.AFR4  as LogisticStdw4
                , m.LogisticPerYearStdw * m.AFR5  as LogisticStdw5

                , m.LogisticPerYear * m.AFR1  as Logistic1
                , m.LogisticPerYear * m.AFR2  as Logistic2
                , m.LogisticPerYear * m.AFR3  as Logistic3
                , m.LogisticPerYear * m.AFR4  as Logistic4
                , m.LogisticPerYear * m.AFR5  as Logistic5
                , m.LogisticPerYear * m.AFRP1 as Logistic1P

        from CostCte m
    )
    , CostCte2_2 as (
        select    m.*

                , case when m.StdWarranty >= 1 then m.TaxAndDutiesOrZero * m.mat1 else 0 end as tax1
                , case when m.StdWarranty >= 2 then m.TaxAndDutiesOrZero * m.mat2 else 0 end as tax2
                , case when m.StdWarranty >= 3 then m.TaxAndDutiesOrZero * m.mat3 else 0 end as tax3
                , case when m.StdWarranty >= 4 then m.TaxAndDutiesOrZero * m.mat4 else 0 end as tax4
                , case when m.StdWarranty >= 5 then m.TaxAndDutiesOrZero * m.mat5 else 0 end as tax5
                , 0  as tax1P

                , case when m.StdWarranty >= 1 then 0 else m.TaxAndDutiesOrZero * m.matO1 end as taxO1
                , case when m.StdWarranty >= 2 then 0 else m.TaxAndDutiesOrZero * m.matO2 end as taxO2
                , case when m.StdWarranty >= 3 then 0 else m.TaxAndDutiesOrZero * m.matO3 end as taxO3
                , case when m.StdWarranty >= 4 then 0 else m.TaxAndDutiesOrZero * m.matO4 end as taxO4
                , case when m.StdWarranty >= 5 then 0 else m.TaxAndDutiesOrZero * m.matO5 end as taxO5
                , m.matO1P * m.AFRP1 as taxO1P

                , m.mat1  + m.matO1                     as matCost1
                , m.mat2  + m.matO2                     as matCost2
                , m.mat3  + m.matO3                     as matCost3
                , m.mat4  + m.matO4                     as matCost4
                , m.mat5  + m.matO5                     as matCost5
                , m.mat1P + m.matO1P                    as matCost1P

                , m.TaxAndDutiesOrZero * (m.mat1  + m.matO1)  as TaxAndDuties1
                , m.TaxAndDutiesOrZero * (m.mat2  + m.matO2)  as TaxAndDuties2
                , m.TaxAndDutiesOrZero * (m.mat3  + m.matO3)  as TaxAndDuties3
                , m.TaxAndDutiesOrZero * (m.mat4  + m.matO4)  as TaxAndDuties4
                , m.TaxAndDutiesOrZero * (m.mat5  + m.matO5)  as TaxAndDuties5
                , m.TaxAndDutiesOrZero * (m.mat1P + m.matO1P) as TaxAndDuties1P

        from CostCte2 m
    )
    , CostCte3 as (
        select    
                  m.*

                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect2
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect3
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect4
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect5
                , Hardware.AddMarkupFactorOrFixValue(m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.ReinsuranceOrZero + m.AvailabilityFeeOrZero, m.MarkupFactorOtherCost, m.MarkupOtherCost)  as OtherDirect1P

                , case when m.StdWarranty >= 1 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw1, m.ServiceSupport, m.LogisticStdw1, m.tax1, m.AFR1, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty1
                , case when m.StdWarranty >= 2 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw2, m.ServiceSupport, m.LogisticStdw2, m.tax2, m.AFR2, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty2
                , case when m.StdWarranty >= 3 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw3, m.ServiceSupport, m.LogisticStdw3, m.tax3, m.AFR3, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty3
                , case when m.StdWarranty >= 4 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw4, m.ServiceSupport, m.LogisticStdw4, m.tax4, m.AFR4, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty4
                , case when m.StdWarranty >= 5 
                        then Hardware.CalcLocSrvStandardWarranty(m.FieldServiceCostStdw5, m.ServiceSupport, m.LogisticStdw5, m.tax5, m.AFR5, 1 + m.MarkupFactorStandardWarranty, m.MarkupStandardWarranty)
                        else 0 
                    end as LocalServiceStandardWarranty5
                , 0     as LocalServiceStandardWarranty1P

        from CostCte2_2 m
    )
    , CostCte4 as (
        select m.*
             , m.mat1 + m.LocalServiceStandardWarranty1 as Credit1
             , m.mat2 + m.LocalServiceStandardWarranty2 as Credit2
             , m.mat3 + m.LocalServiceStandardWarranty3 as Credit3
             , m.mat4 + m.LocalServiceStandardWarranty4 as Credit4
             , m.mat5 + m.LocalServiceStandardWarranty5 as Credit5
             , 0 as Credit1P
        from CostCte3 m
    )
    , CostCte5 as (
        select m.*

             , m.FieldServiceCost1  + m.ServiceSupport + m.matCost1  + m.Logistic1  + m.TaxAndDuties1  + m.ReinsuranceOrZero + m.OtherDirect1  + m.AvailabilityFeeOrZero - m.Credit1  as ServiceTP1
             , m.FieldServiceCost2  + m.ServiceSupport + m.matCost2  + m.Logistic2  + m.TaxAndDuties2  + m.ReinsuranceOrZero + m.OtherDirect2  + m.AvailabilityFeeOrZero - m.Credit2  as ServiceTP2
             , m.FieldServiceCost3  + m.ServiceSupport + m.matCost3  + m.Logistic3  + m.TaxAndDuties3  + m.ReinsuranceOrZero + m.OtherDirect3  + m.AvailabilityFeeOrZero - m.Credit3  as ServiceTP3
             , m.FieldServiceCost4  + m.ServiceSupport + m.matCost4  + m.Logistic4  + m.TaxAndDuties4  + m.ReinsuranceOrZero + m.OtherDirect4  + m.AvailabilityFeeOrZero - m.Credit4  as ServiceTP4
             , m.FieldServiceCost5  + m.ServiceSupport + m.matCost5  + m.Logistic5  + m.TaxAndDuties5  + m.ReinsuranceOrZero + m.OtherDirect5  + m.AvailabilityFeeOrZero - m.Credit5  as ServiceTP5
             , m.FieldServiceCost1P + m.ServiceSupport + m.matCost1P + m.Logistic1P + m.TaxAndDuties1P + m.ReinsuranceOrZero + m.OtherDirect1P + m.AvailabilityFeeOrZero - m.Credit1P as ServiceTP1P

        from CostCte4 m
    )
    , CostCte6 as (
        select m.*
             , m.ServiceTP1  - m.OtherDirect1  as ServiceTC1
             , m.ServiceTP2  - m.OtherDirect2  as ServiceTC2
             , m.ServiceTP3  - m.OtherDirect3  as ServiceTC3
             , m.ServiceTP4  - m.OtherDirect4  as ServiceTC4
             , m.ServiceTP5  - m.OtherDirect5  as ServiceTC5
             , m.ServiceTP1P - m.OtherDirect1P as ServiceTC1P
        from CostCte5 m
    )    
    select m.Id

         --SLA

         , m.CountryId
         , m.Country
         , m.WgId
         , m.Wg
         , m.AvailabilityId
         , m.Availability
         , m.DurationId
         , m.Duration
         , m.Year
         , m.IsProlongation
         , m.ReactionTimeId
         , m.ReactionTime
         , m.ReactionTypeId
         , m.ReactionType
         , m.ServiceLocationId
         , m.ServiceLocation
         , m.ProActiveSlaId
         , m.ProActiveSla

         , m.StdWarranty

         --Cost

         , m.AvailabilityFee * m.Year as AvailabilityFee
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.tax1, m.tax2, m.tax3, m.tax4, m.tax5, m.tax1P) as TaxAndDutiesW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.taxO1, m.taxO2, m.taxO3, m.taxO4, m.taxO5, m.taxO1P) as TaxAndDutiesOow
         , m.Reinsurance
         , m.ProActive
         , m.ServiceSupportCost

         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.mat1, m.mat2, m.mat3, m.mat4, m.mat5, m.mat1P) as MaterialW
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.matO1, m.matO2, m.matO3, m.matO4, m.matO5, m.matO1P) as MaterialOow
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.FieldServiceCost1, m.FieldServiceCost2, m.FieldServiceCost3, m.FieldServiceCost4, m.FieldServiceCost5, m.FieldServiceCost1P) as FieldServiceCost
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Logistic1, m.Logistic2, m.Logistic3, m.Logistic4, m.Logistic5, m.Logistic1P) as Logistic
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.OtherDirect1, m.OtherDirect2, m.OtherDirect3, m.OtherDirect4, m.OtherDirect5, m.OtherDirect1P) as OtherDirect
         
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.LocalServiceStandardWarranty1, m.LocalServiceStandardWarranty2, m.LocalServiceStandardWarranty3, m.LocalServiceStandardWarranty4, m.LocalServiceStandardWarranty5, m.LocalServiceStandardWarranty1P) as LocalServiceStandardWarranty
         
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.Credit1, m.Credit2, m.Credit3, m.Credit4, m.Credit5, m.Credit1P) as Credits


         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTC1, m.ServiceTC2, m.ServiceTC3, m.ServiceTC4, m.ServiceTC5, m.ServiceTC1P) as ServiceTC
         , Hardware.CalcByDur(m.Year, m.IsProlongation, m.ServiceTP1, m.ServiceTP2, m.ServiceTP3, m.ServiceTP4, m.ServiceTP5, m.ServiceTP1P) as ServiceTP

         , m.ServiceTC1
         , m.ServiceTC2
         , m.ServiceTC3
         , m.ServiceTC4
         , m.ServiceTC5
         , m.ServiceTC1P

         , m.ServiceTP1
         , m.ServiceTP2
         , m.ServiceTP3
         , m.ServiceTP4
         , m.ServiceTP5
         , m.ServiceTP1P

         , m.ListPrice
         , m.DealerDiscount
         , m.DealerPrice
         , m.ServiceTCManual
         , m.ServiceTPManual
         , m.ChangeUserName
         , m.ChangeUserEmail

         , m.ServiceTP_Released

         , m.SlaHash

       from CostCte6 m
)

go

ALTER FUNCTION [Hardware].[GetCosts](
    @approved bit,
   	@cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int
)
RETURNS TABLE 
AS
RETURN 
(
    select Id

         , Country
         , Wg
         , Availability
         , Duration
         , ReactionTime
         , ReactionType
         , ServiceLocation
         , ProActiveSla

         , StdWarranty

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

         , ListPrice
         , DealerDiscount
         , DealerPrice
         , ServiceTCManual
         , ServiceTPManual
         , ChangeUserName
         , ChangeUserEmail

         ,ServiceTP_Released

    from Hardware.GetCostsFull(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit)
)

go

ALTER PROCEDURE [Hardware].[SpGetCosts]
    @approved bit,
    @local bit,
    @cnt dbo.ListID readonly,
    @wg dbo.ListID readonly,
    @av dbo.ListID readonly,
    @dur dbo.ListID readonly,
    @reactiontime dbo.ListID readonly,
    @reactiontype dbo.ListID readonly,
    @loc dbo.ListID readonly,
    @pro dbo.ListID readonly,
    @lastid bigint,
    @limit int,
    @total int output
AS
BEGIN

    SET NOCOUNT ON;

	declare @isEmptyCnt    bit = Portfolio.IsListEmpty(@cnt);
    declare @isEmptyWG    bit = Portfolio.IsListEmpty(@wg);
    declare @isEmptyAv    bit = Portfolio.IsListEmpty(@av);
    declare @isEmptyDur   bit = Portfolio.IsListEmpty(@dur);
    declare @isEmptyRType bit = Portfolio.IsListEmpty(@reactiontype);
    declare @isEmptyRTime bit = Portfolio.IsListEmpty(@reactiontime);
    declare @isEmptyLoc   bit = Portfolio.IsListEmpty(@loc);
    declare @isEmptyPro   bit = Portfolio.IsListEmpty(@pro);

    select @total = COUNT(id)
    from Portfolio.LocalPortfolio m
   where   (@isEmptyCnt = 1 or CountryId in (select id from @cnt))
		AND (@isEmptyWG = 1 or WgId in (select id from @wg))
		AND (@isEmptyAv = 1 or AvailabilityId in (select id from @av))
		AND (@isEmptyDur = 1 or DurationId in (select id from @dur))
		AND (@isEmptyRTime = 1 or ReactionTimeId in (select id from @reactiontime))
		AND (@isEmptyRType = 1 or ReactionTypeId in (select id from @reactiontype))
		AND (@isEmptyLoc = 1 or ServiceLocationId in (select id from @loc))
		AND (@isEmptyPro = 1 or ProActiveSlaId in (select id from @pro))


    declare @cur nvarchar(max);
    declare @exchange float;

    if @local = 1
    begin
    
        --convert values from EUR to local

        select costs.Id

             , Country
             , cur.Name as Currency
             , er.Value as ExchangeRate

             , Wg
             , Availability
             , Duration
             , ReactionTime
             , ReactionType
             , ServiceLocation
             , ProActiveSla

             , StdWarranty

             --Cost

             , AvailabilityFee               * er.Value  as AvailabilityFee 
             , TaxAndDutiesW                 * er.Value  as TaxAndDutiesW
             , TaxAndDutiesOow               * er.Value  as TaxAndDutiesOow
             , Reinsurance                   * er.Value  as Reinsurance
             , ProActive                     * er.Value  as ProActive
             , ServiceSupportCost            * er.Value  as ServiceSupportCost

             , MaterialW                     * er.Value  as MaterialW
             , MaterialOow                   * er.Value  as MaterialOow
             , FieldServiceCost              * er.Value  as FieldServiceCost
             , Logistic                      * er.Value  as Logistic
             , OtherDirect                   * er.Value  as OtherDirect
             , LocalServiceStandardWarranty  * er.Value  as LocalServiceStandardWarranty
             , Credits                       * er.Value  as Credits
             , ServiceTC                     * er.Value  as ServiceTC
             , ServiceTP                     * er.Value  as ServiceTP

             , ServiceTCManual               * er.Value  as ServiceTCManual
             , ServiceTPManual               * er.Value  as ServiceTPManual

             , ServiceTP_Released            * er.Value as ServiceTP_Released

             , ListPrice                     * er.Value  as ListPrice
             , DealerPrice                   * er.Value  as DealerPrice
             , DealerDiscount                             as DealerDiscount

             , ChangeUserName                             as ChangeUserName
             , ChangeUserEmail                            as ChangeUserEmail

        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) costs
        join [InputAtoms].Country c on c.Name = costs.Country
        join [References].Currency cur on cur.Id = c.CurrencyId
        join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        order by Id 
        
    end
    else
    begin

        select  cur.Name as Currency
             , er.Value as ExchangeRate, 
			 costs.*
        from Hardware.GetCosts(@approved, @cnt, @wg, @av, @dur, @reactiontime, @reactiontype, @loc, @pro, @lastid, @limit) costs
		join [InputAtoms].Country c on c.Name = costs.Country
		join [References].Currency cur on cur.Id = c.CurrencyId
		join [References].ExchangeRate er on er.CurrencyId = c.CurrencyId
        order by Id
    end
END

go

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
    with CurrencyCte as (
        select   case when @local = 1 then cur.Name else 'EUR' end as Currency
               , case when @local = 1 then er.Value else 1     end as Exchange
        from [References].Currency cur
        join [References].ExchangeRate er on er.CurrencyId = cur.Id
        where cur.Id = (select CurrencyId from InputAtoms.Country where id  in (select id from @country))
    )
    select    Country
            , case when @local = 1 then cur.Name else 'EUR' end as Currency

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

    from Hardware.GetCosts(@approved, @country, @wg, @availability, @duration, @reactiontime, @reactiontype, @servicelocation, @proactive, -1, -1)
	join [References].Currency cur on cur.Id in (select CurrencyId from InputAtoms.Country where id in (select id from @country))
	join [References].ExchangeRate er on er.CurrencyId = cur.Id
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
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'approved', 'Approved');
set @index = @index + 1;
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, 3, 'local', 'Local currency');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'country' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'country', 'Country');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'wg' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'wg', 'Asset(WG)');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'availability' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'availability', 'Availability');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'duration' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'duration', 'Duration');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'reactiontime' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'reactiontime', 'Reaction time');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'reactiontype' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'reactiontype', 'Reaction type');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'servicelocation' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'servicelocation', 'Service location');

set @index = @index + 1;
set @filterTypeId = (select Id from Report.ReportFilterType where Name = 'proactive' and MultiSelect=1)
insert into Report.ReportFilter(ReportId, [Index], TypeId, Name, Text) values(@reportId, @index, @filterTypeId, 'proactive', 'ProActive');



go