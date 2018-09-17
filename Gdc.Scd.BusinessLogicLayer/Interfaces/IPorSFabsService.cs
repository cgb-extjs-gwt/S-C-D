using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IPorSFabsService
    {
        bool UploadSFabs(IDictionary<string, string> sfabs, 
                         IEnumerable<Pla> plas, DateTime modifiedDate);

        bool DeactivateSFabs(IDictionary<string, string> sfabs, DateTime modifiedDate);
    }
}
