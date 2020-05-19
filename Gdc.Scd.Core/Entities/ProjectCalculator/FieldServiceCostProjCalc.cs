namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class FieldServiceCostProjCalc
    {
        public double? TimeAndMaterialShare { get; set; }

        public double? TravelCost { get; set; }

        public double? LabourCost { get; set; }

        public double? PerformanceRate { get; set; }

        /// <summary>
        /// Travel Time (MTTT)
        /// </summary>
        public double? TravelTime { get; set; }
    }
}
