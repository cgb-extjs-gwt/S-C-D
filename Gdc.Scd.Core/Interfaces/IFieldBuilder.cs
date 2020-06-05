using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Core.Interfaces
{
    public interface IFieldBuilder
    {
        FieldMeta BuildAutoApprovedField(string costElementId);
    }
}
