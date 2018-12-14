namespace Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix
{
    public class CapabilityMatrixDto
    {
        public long Id { get; set; }

        public string Country { get; set; }

        public string Wg { get; set; }

        public string Availability { get; set; }

        public string Duration { get; set; }

        public string ReactionType { get; set; }

        public string ReactionTime { get; set; }

        public string ServiceLocation { get; set; }

        public string ProActive { get; set; }

        public bool IsGlobalPortfolio { get; set; }

        public bool IsMasterPortfolio { get; set; }

        public bool IsCorePortfolio { get; set; }
    }
}
