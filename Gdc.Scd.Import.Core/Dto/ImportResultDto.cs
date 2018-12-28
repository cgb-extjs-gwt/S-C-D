using Gdc.Scd.Core.Meta.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Dto
{
    public class ImportResultDto
    {
        public bool Skipped { get; set; }
        public IEnumerable<UpdateQueryOption> UpdateOptions { get; set; }

        public ImportResultDto()
        {
            Skipped = false;
        }
    }
}
