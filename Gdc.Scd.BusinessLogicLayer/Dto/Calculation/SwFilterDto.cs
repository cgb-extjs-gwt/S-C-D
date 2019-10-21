namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class SwFilterDto
    {
        public long[] Digit { get; set; }
        public long? Sog { get; set; }
        public long[] Country { get; set; }
        public long[] Availability { get; set; }
        public long[] Year { get; set; }
        public long[] Duration { get; set; }

        public bool Approved { get; set; }

        public int Page { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
    }
}
