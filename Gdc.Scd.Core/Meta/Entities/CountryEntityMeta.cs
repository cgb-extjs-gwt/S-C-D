using System;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CountryEntityMeta : NamedEntityMeta
    {
        public SimpleFieldMeta QualityGateGroup { get; } = new SimpleFieldMeta(nameof(Country.QualityGateGroup), TypeCode.String);

        public CountryEntityMeta() : 
            base(MetaConstants.CountryInputLevelName, new SimpleFieldMeta(MetaConstants.NameFieldKey, TypeCode.String), MetaConstants.InputLevelSchema)
        {
        }
    }
}
