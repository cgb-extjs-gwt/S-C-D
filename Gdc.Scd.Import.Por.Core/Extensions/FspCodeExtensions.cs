using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Import;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Extensions
{
    public static class FspCodeExtensions
    {
        public static SlaDto MapFspCodeToSla(this SCD2_v_SAR_new_codes porFspCode, 
            SlaDictsDto sla,
            Dictionary<string, long> proActive = null,
            bool mapProActive = false,
            IEnumerable<string> proActiveServiceTypes = null)
        {
            if (String.IsNullOrEmpty(porFspCode.Atom_Location) || 
                !sla.Locations.ContainsKey(porFspCode.Atom_Location))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_Duration) || 
                !sla.Duration.ContainsKey(porFspCode.Atom_Duration))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_R_Time) ||  
                !sla.ReactionTime.ContainsKey(porFspCode.Atom_R_Time))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_R_Type) ||  
                !sla.ReactionType.ContainsKey(porFspCode.Atom_R_Type))
                return null;

            if (String.IsNullOrEmpty(porFspCode.Atom_Availability) ||  
                !sla.Availability.ContainsKey(porFspCode.Atom_Availability))
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
                Availability = sla.Availability[porFspCode.Atom_Availability],
                Duration = sla.Duration[porFspCode.Atom_Duration],
                ReactionTime = sla.ReactionTime[porFspCode.Atom_R_Time],
                ReactionType = sla.ReactionType[porFspCode.Atom_R_Type],
                ServiceLocation = sla.Locations[porFspCode.Atom_Location],
                ProActive = proactiveId
            };
        }
    }
}
