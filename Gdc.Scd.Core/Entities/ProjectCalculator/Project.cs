using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    [Table("Project", Schema = MetaConstants.ProjectCalculatorSchema)]
    public class Project : IIdentifiable
    {
        public long Id { get; set; }

        public Wg Wg { get; set; }

        public long WgId { get; set; }
        
        public Country Country { get; set; }

        public long CountryId { get; set; }

        public AvailabilityProjCalc Availability { get; set; }

        public ReactionTimeProjCalc ReactionTime { get; set; }

        public ReactionType ReactionType { get; set; }

        public long ReactionTypeId { get; set; }

        public ServiceLocation ServiceLocation { get; set; }

        public long ServiceLocationId { get; set; }

        public DurationProjCalc Duration { get; set; }

        //public HwHddFspCodeTranslation FspCode { get; set; }

        public bool IsCalculated { get; set; }

        public List<AfrProjCalc> Afrs { get; set; }

        public double? OnsiteHourlyRates { get; set; }

        public FieldServiceCostProjCalc FieldServiceCost { get; set; }

        public ReinsuranceProjCalc Reinsurance { get; set; }

        public MarkupOtherCostsProjCalc MarkupOtherCosts { get; set; }

        public LogisticsCostsProjCalc LogisticsCosts { get; set; }

        public AvailabilityFeeProjCalc AvailabilityFee { get; set; }

        //#region FieldServiceCost
        //public double? TimeAndMaterialShare { get; set; }

        //public double? TravelCost { get; set; }

        //public double? LabourCost { get; set; }

        //public double? PerformanceRate { get; set; }

        ///// <summary>
        ///// Travel Time (MTTT)
        ///// </summary>
        //public double? TravelTime { get; set; }
        //#endregion

        //#region Reinsurance
        //public double? ReinsuranceFlatfee { get; set; }

        //public double? ReinsuranceUpliftFactor { get; set; }
        //#endregion

        //#region MarkupOtherCosts
        ///// <summary>
        ///// Markup factor for other cost (%)
        ///// </summary>
        //public double? MarkupFactor { get; set; }

        ///// <summary>
        ///// Prolongation markup factor for other cost (%)
        ///// </summary>
        //public double? ProlongationMarkupFactor { get; set; }

        ///// <summary>
        ///// Prolongation markup for other cost
        ///// </summary>
        //public double? ProlongationMarkup { get; set; }
        //#endregion

        //#region LogisticsCosts
        //public double? ExpressDelivery { get; set; }

        //public double? HighAvailabilityHandling { get; set; }

        //public double? StandardDelivery { get; set; }

        //public double? StandardHandling { get; set; }

        //public double? ReturnDeliveryFactory { get; set; }

        //public double? TaxiCourierDelivery { get; set; }
        //#endregion

        //#region AvailabilityFee
        //public double? InstalledBaseHighAvailability { get; set; }

        //public double? TotalLogisticsInfrastructureCost { get; set; }

        //public double? StockValueFj { get; set; }

        //public double? StockValueMv { get; set; }

        //public double? AverageContractDuration { get; set; }

        //public double? CostPerKit { get; set; }

        //public double? CostPerKitJapanBuy { get; set; }

        //public double? MaxQty { get; set; }

        //public double? JapanBuy { get; set; }
        //#endregion
    }
}
