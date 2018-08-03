namespace Gdc.Scd.BusinessLogicLayer.Dto.CapabilityMatrix
{
    public class CapabilityMatrixFilterDto
    {
        public long? Country {get;set;}

        public long? Wg {get;set;}
        public long? Availability {get;set;}
        public long? Duration {get;set;}
        public long? ReactionType {get;set;}
        public long? ReactionTime {get;set;}
        public long? ServiceLocation {get;set;}

        public bool? IsGlobalPortfolio {get;set;}
        public bool? IsMasterPortfolio {get;set;}
        public bool? IsCorePortfolio {get;set;}
}
}
