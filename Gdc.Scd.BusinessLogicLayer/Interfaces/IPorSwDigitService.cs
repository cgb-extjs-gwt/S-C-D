using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.External.Por;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPorSwDigitService
    {
        bool UploadSwDigits(Dictionary<string, string> swInfo, IEnumerable<Sog> sogs, 
            DateTime modifiedDateTime);

        bool Deactivate(Dictionary<string, string> swInfo, DateTime modifiedDateTime);
    }
}
