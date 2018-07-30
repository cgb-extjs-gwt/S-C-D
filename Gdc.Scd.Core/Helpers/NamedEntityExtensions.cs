using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Core.Helpers
{
    public static class NamedEntityExtensions
    {
        public static string GetName(this NamedId entity)
        {
            return entity == null ? null : entity.Name;
        }
    }
}
