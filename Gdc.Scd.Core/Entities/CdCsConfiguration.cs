using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Entities
{
    [Table("CdCsConfiguration", Schema = MetaConstants.ReportSchema)]
    public class CdCsConfiguration : IIdentifiable
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public Country Country { get; set; }
        public long CountryId { get; set; }
        public string FileWebUrl { get; set; }
        public string FileFolderUrl { get; set; }
    }
}
