namespace Gdc.Scd.BusinessLogicLayer.Dto.Portfolio
{
    public class PortfolioRuleSetDto
    {
        public long? CountryId { get; set; }

        public long[] Wgs { get; set; }
        public long[] Availabilities { get; set; }
        public long[] Durations { get; set; }
        public long[] ReactionTypes { get; set; }
        public long[] ReactionTimes { get; set; }
        public long[] ServiceLocations { get; set; }
        public long[] ProActives { get; set; }

        public bool IsGlobalPortfolio { get; set; }
        public bool IsMasterPortfolio { get; set; }
        public bool IsCorePortfolio { get; set; }
    }
}
