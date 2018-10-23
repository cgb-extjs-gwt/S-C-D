using Gdc.Scd.Import.Por.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IPorSwProActiveService
    {
        bool UploadSwProactiveInfo(SwProActiveDto model);
    }
}
