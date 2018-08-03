namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class SqlBuilderContext
    {
        private int parameterIndex;

        public string GetNewParameterName()
        {
            return $"parameter_{parameterIndex++}";
        }
    }
}
