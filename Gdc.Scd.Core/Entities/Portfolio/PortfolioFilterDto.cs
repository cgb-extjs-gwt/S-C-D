namespace Gdc.Scd.Core.Entities.Portfolio
{
    public class PortfolioFilterDto
    {
        public long[] Country { get; set; }

        public long[] Wg { get; set; }
        public long[] Availability { get; set; }
        public long[] Duration { get; set; }
        public long[] ReactionType { get; set; }
        public long[] ReactionTime { get; set; }
        public long[] ServiceLocation { get; set; }
        public long[] ProActiveSla { get; set; }

        public bool? IsGlobalPortfolio { get; set; }
        public bool? IsMasterPortfolio { get; set; }
        public bool? IsCorePortfolio { get; set; }

        public int Start { get; set; }
        public int Limit { get; set; }
    }
}
