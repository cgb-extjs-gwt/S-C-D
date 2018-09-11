using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class CaseItem
    {
        public ISqlBuilder When { get; set; }

        public ISqlBuilder Then { get; set; }
    }
}
