using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Core.Meta.Interfaces;
using Gdc.Scd.Tests.Common.CostBlock.Entities;

namespace Gdc.Scd.Tests.Common.CostBlock.Impl
{
    public class CoordinateEntityMetaProvider : ICoordinateEntityMetaProvider
    {
        public IEnumerable<NamedEntityMeta> GetCoordinateEntityMetas()
        {
            var inputLevel1Meta = new NamedEntityMeta(nameof(RelatedInputLevel1), MetaConstants.InputLevelSchema);

            var inputLevel2Meta = new NamedEntityMeta(nameof(RelatedInputLevel2), MetaConstants.InputLevelSchema);
            inputLevel2Meta.Fields.Add(ReferenceFieldMeta.Build("RelatedInputLevel1Id", inputLevel1Meta));

            var inputLevel3Meta = new NamedEntityMeta(nameof(RelatedInputLevel3), MetaConstants.InputLevelSchema);
            inputLevel3Meta.Fields.Add(ReferenceFieldMeta.Build("RelatedInputLevel2Id", inputLevel2Meta));

            return new[]
            {
                inputLevel1Meta,
                inputLevel2Meta,
                inputLevel3Meta
            };
        }
    }
}
