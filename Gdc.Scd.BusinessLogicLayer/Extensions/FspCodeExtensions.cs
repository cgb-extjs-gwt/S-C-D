using Gdc.Scd.BusinessLogicLayer.Dto.Import;
using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Extensions
{
    public static class FspCodeExtensions
    {
        public static SlaDto MapFspCodeToSla(this SCD2_v_SAR_new_codes porFspCode, 
            Dictionary<string, long> serviceLocations,
            Dictionary<string, long> durations,
            Dictionary<string, long> reactionTimeValues,
            Dictionary<string, long> reactionTypeValues,
            Dictionary<string, long> availabilityValues, 
            Dictionary<string, long> proActive = null,
            bool mapProActive = false,
            IEnumerable<string> proActiveServiceTypes = null)
        {
            if (String.IsNullOrEmpty(porFspCode.Atom_Location) || 
                !serviceLocations.ContainsKey(porFspCode.Atom_Location))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_Duration) || 
                !durations.ContainsKey(porFspCode.Atom_Duration))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_R_Time) ||  
                !reactionTimeValues.ContainsKey(porFspCode.Atom_R_Time))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_R_Type) ||  
                !reactionTypeValues.ContainsKey(porFspCode.Atom_R_Type))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_Availability) ||  
                !availabilityValues.ContainsKey(porFspCode.Atom_Availability))
                return null;

            long? proactiveId = null;

            if (mapProActive)
            {
                if (proActiveServiceTypes != null && proActiveServiceTypes != null)
                {
                    if (proActiveServiceTypes.Contains(porFspCode.SCD_ServiceType, StringComparer.OrdinalIgnoreCase)
                        && !proActive.ContainsKey(porFspCode.SecondSLA))
                    {
                        return null;
                    }

                    proactiveId = proActive[porFspCode.SecondSLA];
                }
            }

            return new SlaDto
            {
                Availability = availabilityValues[porFspCode.Atom_Availability],
                Duration = durations[porFspCode.Atom_Duration],
                ReactionTime = reactionTimeValues[porFspCode.Atom_R_Time],
                ReactionType = reactionTypeValues[porFspCode.Atom_R_Type],
                ServiceLocation = serviceLocations[porFspCode.Atom_Location],
                ProActive = proactiveId
            };
        }
    }
}
