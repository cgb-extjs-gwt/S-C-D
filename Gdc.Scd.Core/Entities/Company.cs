using Gdc.Scd.Core.Meta.Constants;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities
{
    [Table(MetaConstants.CompanyInputLevelName, Schema = MetaConstants.InputLevelSchema)]
    public class Company : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public List<Pla> Plas { get; set; }
    }
}
