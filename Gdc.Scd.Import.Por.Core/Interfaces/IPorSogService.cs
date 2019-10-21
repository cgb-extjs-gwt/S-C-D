using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSogService
    {
        bool UploadSogs(IEnumerable<SogPorDto> sogs, 
            IEnumerable<Pla> plas,
            DateTime modifiedDate, 
            List<UpdateQueryOption> updateOptions);

        bool DeactivateSogs(IEnumerable<SogPorDto> sogs, DateTime modifiedDatetime);
    }
}
