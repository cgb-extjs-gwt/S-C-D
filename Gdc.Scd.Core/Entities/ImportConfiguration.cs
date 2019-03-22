using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Entities
{
    [Table("Configuration", Schema = MetaConstants.ImportSchema)]
    public class ImportConfiguration : NamedId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public ImportMode ImportMode { get; set; }
        public DateTime? ProcessedDateTime { get; set; }
        public string ProcessedFilesPath { get; set; }
        public string Delimeter { get; set; }
        public bool HasHeader { get; set; }
        public string Culture { get; set; }
    }
}
