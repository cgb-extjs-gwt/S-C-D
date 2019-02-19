using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class QueryData
    {
        public string Sql { get; set; }

        public CommandParameterInfo[]  Parameters { get; set; }
    }
}
