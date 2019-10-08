namespace Gdc.Scd.Tests.Util
{
    public static class CsvHelper
    {
        public static int GetInt(this string[] line, int index)
        {
            return int.Parse(line[index]);
        }

        public static double GetDouble(this string[] line, int index)
        {
            return double.Parse(line[index]);
        }

        public static string GetString(this string[] line, int index)
        {
            return line[index];
        }
    }
}
