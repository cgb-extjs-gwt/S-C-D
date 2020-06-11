using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class NamedId : IIdentifiable
    {
        public virtual long Id { get; set; }

        [MustCompare(true, IsIgnoreCase = true)]
        public virtual string Name { get; set; }

        public NamedId()
        { 
        }

        public NamedId(long id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
