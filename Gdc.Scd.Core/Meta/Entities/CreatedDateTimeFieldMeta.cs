using Gdc.Scd.Core.Interfaces;
using System;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CreatedDateTimeFieldMeta : SimpleFieldMeta
    {
        public const string DefaultId = nameof(IDeactivatable.CreatedDateTime);

        public CreatedDateTimeFieldMeta() 
            : base(DefaultId, TypeCode.DateTime)
        {
        }
    }
}
