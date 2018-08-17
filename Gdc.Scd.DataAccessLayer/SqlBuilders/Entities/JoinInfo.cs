using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class JoinInfo
    {
        public BaseEntityMeta Meta { get; set; }

        public string ReferenceFieldName { get; set; }

        public string Alias { get; set; }
    }
}
