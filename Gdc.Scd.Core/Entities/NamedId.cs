using Gdc.Scd.Core.Attributes;
using Gdc.Scd.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Gdc.Scd.Core.Entities
{
    public class NamedId : IIdentifiable
    {
        public virtual long Id { get; set; }

        [MustCompare(true)]
        public virtual string Name { get; set; }
    }
}
