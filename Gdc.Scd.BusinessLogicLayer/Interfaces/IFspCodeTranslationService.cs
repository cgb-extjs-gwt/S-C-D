using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IFspCodeTranslationService
    {
        bool UploadFspCodes(IEnumerable<v_SAR_new_codes> fspCodes,
            IEnumerable<Country> countries,
            IEnumerable<Sog> sogs,
            IEnumerable<Wg> wgs,
            IEnumerable<ServiceLocation> serviceLocations,
            IEnumerable<Duration> durations,
            IEnumerable<ReactionTime> reactionTimeValues,
            IEnumerable<ReactionType> reactionTypeValues,
            IEnumerable<Availability> availabilityValues,
            IEnumerable<ProActiveSla> proActiveValues,
            string[] allowedProactiveTypes,
            string[] allowedStandardWarranties,
            string[] otherServiceTypes,
            DateTime modifiedDate);

        bool DeactivateFspCodes(IEnumerable<v_SAR_new_codes> fspCodes, DateTime modifiedDatetime);
    }
}
