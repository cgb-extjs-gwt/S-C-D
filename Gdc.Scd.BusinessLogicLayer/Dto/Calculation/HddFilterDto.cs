namespace Gdc.Scd.BusinessLogicLayer.Dto.Calculation
{
    public class HddFilterDto
    {
        public long[] Wg { get; set; }

        public bool Approved { get; set; }

        public int Page { get; set; }

        public int Start { get; set; }

        public int Limit { get; set; }
    }
}
