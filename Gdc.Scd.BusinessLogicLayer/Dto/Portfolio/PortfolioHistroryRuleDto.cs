namespace Gdc.Scd.BusinessLogicLayer.Dto.Portfolio
{
    public class PortfolioHistroryRuleDto
    {
        public string Country { get; set; }

        public string[] Wgs { get; set; }
        public string[] Availabilities { get; set; }
        public string[] Durations { get; set; }
        public string[] ReactionTypes { get; set; }
        public string[] ReactionTimes { get; set; }
        public string[] ServiceLocations { get; set; }
        public string[] ProActives { get; set; }

        public bool IsGlobalPortfolio { get; set; }
        public bool IsMasterPortfolio { get; set; }
        public bool IsCorePortfolio { get; set; }
    }
}
