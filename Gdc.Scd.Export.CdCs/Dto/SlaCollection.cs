using System.Collections.Generic;
using System.Data;

namespace Gdc.Scd.Export.CdCs.Dto
{
    public class SlaCollection : List<SlaDto>
    {
        private DataTable dt;

        public SlaCollection(int count) : base(count) { }

        public DataTable AsTable()
        {
            if (dt == null)
            {
                var tbl = new DataTable();

                tbl.Columns.Add("Wg", typeof(string));
                tbl.Columns.Add("Availability", typeof(string));
                tbl.Columns.Add("Duration", typeof(string));
                tbl.Columns.Add("ReactionTime", typeof(string));
                tbl.Columns.Add("ReactionType", typeof(string));
                tbl.Columns.Add("ServiceLocation", typeof(string));
                tbl.Columns.Add("ProActiveSla", typeof(string));

                var rows = tbl.Rows;
                for (var i = 0; i < Count; i++)
                {
                    var s = this[i];
                    rows.Add(s.WarrantyGroup, s.Availability, s.Duration, s.ReactionTime, s.ReactionType, s.ServiceLocation);
                }

                dt = tbl;
            }
            return dt;
        }
    }
}
