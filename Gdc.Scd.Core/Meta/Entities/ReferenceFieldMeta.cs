using System;
using System.Collections.Generic;
using System.Text;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class ReferenceFieldMeta : FieldMeta
    {
        public string RefEnitityFullName { get; set; }

        public string ValueField { get; set; }

        public string NameField { get; set; }

        public ReferenceFieldMeta(string id) 
            : base(id)
        {
        }
    }
}
