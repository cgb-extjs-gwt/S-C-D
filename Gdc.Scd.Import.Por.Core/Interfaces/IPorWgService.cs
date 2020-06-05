using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.Import.Por.Core.Dto;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorWgService
    {
        (bool ok, List<Wg> added) UploadWgs(IEnumerable<WgPorDto> wgs,
            IEnumerable<Sog> sogs,
            IEnumerable<Pla> plas,
            DateTime modifiedDateTime, 
            List<UpdateQueryOption> updateOptions);

        bool DeactivateWgs(IEnumerable<WgPorDto> wgs, DateTime modifiedDatetime);
        bool ChangeWgTypeToLogistic(IEnumerable<WgPorDto> wgs);
    }
}
