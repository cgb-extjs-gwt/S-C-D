using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.CapabilityMatrix;
using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IHwFspCodeTranslationService
    {
        bool UploadHardware(
            IEnumerable<SCD2_v_SAR_new_codes> hardwareCodes,
            IEnumerable<SCD2_v_SAR_new_codes> proActiveCodes,
            IEnumerable<SCD2_v_SAR_new_codes> stdwCodes,
            IEnumerable<SCD2_LUT_TSP> stdw,
            Dictionary<string, List<long>> countries,
            IEnumerable<Wg> warranties,
            IEnumerable<Sog> sogs,
            Dictionary<string, long> availabilities, Dictionary<string, long> reactionTime,
            Dictionary<string, long> reactionTypes, Dictionary<string, long> locations,
            Dictionary<string, long> durations, Dictionary<string, long> proActive,
            IQueryable<CapabilityMatrix> matrix,
            DateTime createdDateTime,
            IEnumerable<string> proActiveServiceTypes,
            IEnumerable<string> standardWarrantiesServiceTypes,
            IEnumerable<string> otherHardware);

        bool DeactivateFspHardware(IEnumerable<SCD2_v_SAR_new_codes> fspCodes, DateTime modifiedDate);
    }
}
