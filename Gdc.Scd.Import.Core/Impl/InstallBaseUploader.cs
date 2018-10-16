using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Impl
{
    public class InstallBaseUploader : IUploader<InstallBaseDto>
    {
        public void Upload(IEnumerable<InstallBaseDto> items, DateTime modifiedDateTime)
        {
            throw new NotImplementedException();
        }
    }
}
