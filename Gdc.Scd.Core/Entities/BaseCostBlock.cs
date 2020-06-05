using Gdc.Scd.Core.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    public abstract class BaseCostBlock : IIdentifiable, ICostBlockEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime? DeactivatedDateTime { get; set; }
    }
}
