using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ISwFspCodeTranslationService
    {
        bool UploadSoftware(IEnumerable<SCD2_v_SAR_new_codes> softwareCodes,
            IEnumerable<Wg> warranties,
            IEnumerable<SwDigit> swDigits,
            IEnumerable<SCD2_SW_Overview> swInfo,
            Dictionary<string, long> availabilities, Dictionary<string, long> reactionTime,
            Dictionary<string, long> reactionTypes, Dictionary<string, long> locations,
            Dictionary<string, long> durations,
            DateTime createdDateTime, IEnumerable<string> softwareServiceType);

        bool DeactivateFspSoftware(IEnumerable<SCD2_v_SAR_new_codes> fspCodes, DateTime modifiedDate);

    }
}
