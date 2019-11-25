using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Impl;
using Gdc.Scd.Import.Por.Core.Import;
using System;
using System.Collections.Generic;
using System.Linq;

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
            else
            {
                if (proActive != null && proActive.ContainsKey(Impl.PorConstants.SlaNullValue))
                    proactiveId = proActive[Impl.PorConstants.SlaNullValue];
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

        public static List<long> MapFspCodeToWgs(this SCD2_v_SAR_new_codes porFspCode,
            List<Wg> wgs, List<Sog> sogs, ILogger logger)
        {
            List<long> result = new List<long>();

            if (String.IsNullOrEmpty(porFspCode.WG) && String.IsNullOrEmpty(porFspCode.SOG))
            {
                logger.Warn(PorImportLoggingMessage.EMPTY_SOG_WG, porFspCode.Service_Code);
                return result;
            }
            //If FSP Code is binded to SOG
            if (String.IsNullOrEmpty(porFspCode.WG))
            {
                var sog = sogs.FirstOrDefault(s => s.Name == porFspCode.SOG);
                if (sog == null)
                {
                    logger.Warn(PorImportLoggingMessage.UNKNOWN_SOG, porFspCode.Service_Code, porFspCode.SOG);
                    return result;
                }

                result.AddRange(wgs.Where(w => w.SogId == sog.Id).Select(w => w.Id));
            }

            //FSP Code is binded to WG
            else
            {
                var wg = wgs.FirstOrDefault(w => w.Name == porFspCode.WG);
                if (wg == null)
                {
                    logger.Warn(PorImportLoggingMessage.UNKNOW_WG, porFspCode.Service_Code, porFspCode.WG);
                    return result;
                }

                result.Add(wg.Id);
            }

            return result;
        }
    }
}
