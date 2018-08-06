using System;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CreatedDateTimeFieldMeta : SimpleFieldMeta
    {
        public const string DefaultId = "CreatedDateTime";

        public CreatedDateTimeFieldMeta() 
            : base(DefaultId, TypeCode.DateTime)
        {
        }
    }
}
