using System.ComponentModel.DataAnnotations.Schema;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public abstract class BaseDisabledEntity : IIdentifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public bool IsDisabled { get; set; }
    }
}
