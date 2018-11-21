namespace Gdc.Scd.BusinessLogicLayer.Dto.AvailabilityFee
{
    public class AdminAvailabilityFeeFilterDto
    {
        public long? Country {get;set;}

        public long? ReactionType {get;set;}
        public long? ReactionTime {get;set;}
        public long? ServiceLocation {get;set;}

        public bool? IsApplicable {get;set;}
}
}
