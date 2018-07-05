using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class NamedId : IIdentifiable
    {
        public virtual long Id { get; set; }

        public virtual string Name { get; set; }
    }
}
