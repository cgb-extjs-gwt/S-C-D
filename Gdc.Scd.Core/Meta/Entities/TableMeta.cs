using Gdc.Scd.Core.Meta.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class TableMeta : IMetaIdentifialble
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Schema { get; set; }
    }
}
