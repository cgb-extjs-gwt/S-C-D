using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using System;
using System.Collections.Generic;
using Gdc.Scd.Import.Por.Core.Dto;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface ISwFspCodeTranslationService
    {
        bool UploadSoftware(SwFspCodeDto model);

    }
}
