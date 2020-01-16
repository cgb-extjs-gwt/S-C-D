using System;
using System.Collections.Generic;
using System.Data;

namespace Gdc.Scd.Export.CdCsJob.Dto
{
    public class SlaCollection : List<SlaDto>
    {
        private static readonly Type TYPE_STR = typeof(string);

        private DataTable dt;

        public SlaCollection(int count) : base(count) { }

        public DataTable AsTable()
        {
            if (dt == null)
            {
                var tbl = new DataTable();

                tbl.Columns.Add("Wg", TYPE_STR);
                tbl.Columns.Add("Availability", TYPE_STR);
                tbl.Columns.Add("Duration", TYPE_STR);
                tbl.Columns.Add("ReactionTime", TYPE_STR);
                tbl.Columns.Add("ReactionType", TYPE_STR);
                tbl.Columns.Add("ServiceLocation", TYPE_STR);
                tbl.Columns.Add("ProActiveSla", TYPE_STR);

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
