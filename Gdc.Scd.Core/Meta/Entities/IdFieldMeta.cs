using System;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class IdFieldMeta : SimpleFieldMeta
    {
        public const string DefaultId = "Id";

        public IdFieldMeta() 
            : base(DefaultId, TypeCode.Int64)
        {
        }
    }
}
