using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.Core.Entities
{
    public class EditItemInfo
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string NameColumn { get; set; }

        public string ValueColumn { get; set; }
    }
}
