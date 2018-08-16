using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gdc.Scd.Core.Entities
{
    [Table("Year", Schema = MetaConstants.DependencySchema)]
    public class Year : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public int Value { get; set; }
    }
}
