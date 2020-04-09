namespace Gdc.Scd.Core.Dto
{
    public class HwFilterDto
    {
        public long[] Country { get; set; }

        public string Fsp { get; set; }

        public bool? HasFsp { get; set; }

        public long[] Wg { get; set; }

        public long[] Availability { get; set; }

        public long[] Duration { get; set; }

        public long[] ReactionType { get; set; }

        public long[] ReactionTime { get; set; }

        public long[] ServiceLocation { get; set; }

        public long[] ProActive { get; set; }

        public bool Approved { get; set; }

        public int Page { get; set; }

        public int Start { get; set; }

        public int Limit { get; set; }
    }
}
