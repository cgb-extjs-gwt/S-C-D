namespace Gdc.Scd.BusinessLogicLayer.Dto.Report
{
    public class ReportColumnDto
    {
        public string Name { get; set; }

        public string Text { get; set; }

        public long TypeId { get; set; }

        public string Type { get; set; }

        public bool AllowNull { get; set; }

        public int Flex { get; set; }

        public string Format { get; set; }

        public bool IsBoolean()
        {
            return string.Compare(Type, "boolean", true) == 0;
        }

        public bool IsEuro()
        {
            return string.Compare(Type, "euro", true) == 0;
        }

        public bool IsMoney()
        {
            return string.Compare(Type, "money", true) == 0;
        }

        public bool IsNumber()
        {
            return string.Compare(Type, "number", true) == 0;
        }

        public bool IsPercent()
        {
            return string.Compare(Type, "percent", true) == 0;
        }

        public bool IsDate()
        {
            return string.Compare(Type, "datetime", true) == 0;
        }
    }
}