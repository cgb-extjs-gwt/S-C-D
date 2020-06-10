namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    public class MarkupOtherCostsProjCalc
    {
        public double? Markup { get; set; }

        /// <summary>
        /// Markup factor for other cost (%)
        /// </summary>
        public double? MarkupFactor { get; set; }

        /// <summary>
        /// Prolongation markup factor for other cost (%)
        /// </summary>
        public double? ProlongationMarkupFactor { get; set; }

        /// <summary>
        /// Prolongation markup for other cost
        /// </summary>
        public double? ProlongationMarkup { get; set; }
    }
}
