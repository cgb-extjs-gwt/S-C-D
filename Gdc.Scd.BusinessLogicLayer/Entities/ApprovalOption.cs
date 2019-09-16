namespace Gdc.Scd.BusinessLogicLayer.Entities
{
    public class ApprovalOption
    {
        public bool IsApproving { get; set; }

        public bool HasQualityGateErrors { get; set; }

        public string QualityGateErrorExplanation { get; set; }

        public bool TurnOffNotification { get; set; }
    }
}
