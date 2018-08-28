using System;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class IdFieldMeta : SimpleFieldMeta
    {
        public static string DefaultId => MetaConstants.IdFieldKey;

        public IdFieldMeta() 
            : base(DefaultId, TypeCode.Int64)
        {
        }
    }
}
