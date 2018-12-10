namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class SwProactiveCostDto
    {
        public string Country { get; set; }

        public string Sog { get; set; }

        public string Year { get; set; }

        public double? ProActive { get; set; }
        public double? ProActive_Approved { get; set; }
    }
}
