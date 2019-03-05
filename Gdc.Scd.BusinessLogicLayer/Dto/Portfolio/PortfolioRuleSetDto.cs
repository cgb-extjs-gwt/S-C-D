namespace Gdc.Scd.BusinessLogicLayer.Dto.Portfolio
{
    public class PortfolioRuleSetDto
    {
        public long[] Countries { get; set; }

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

        public bool IsLocalPortfolio()
        {
            return Len(Countries) > 0;
        }

        public bool IsValid()
        {
            var valid = IsLocalPortfolio() || IsGlobalPortfolio || IsMasterPortfolio || IsCorePortfolio;

            if (valid)
            {
                valid = 0 < Len(Wgs) + Len(Availabilities) + Len(Durations) + Len(ReactionTypes) + Len(ServiceLocations) + Len(ProActives);
            }

            return valid;
        }

        private static int Len(long[] arr)
        {
            return arr == null ? 0 : arr.Length;
        }
    }
}
