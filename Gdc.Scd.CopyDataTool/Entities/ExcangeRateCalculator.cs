namespace Gdc.Scd.CopyDataTool.Entities
{
    public class ExcangeRateCalculator
    {
        public double Coeff { get; }

        public ExcangeRateCalculator(double sourceExchangeRate, double targetExchangeRate)
        {
            this.Coeff = targetExchangeRate / sourceExchangeRate;
        }

        public double Calculate(double value)
        {
            return value * this.Coeff;
        }

        public double? Calculate(double? value)
        {
            return value.HasValue ? this.Calculate(value.Value) : default(double?);
        }
    }
}
