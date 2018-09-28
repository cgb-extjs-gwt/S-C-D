using Gdc.Scd.BusinessLogicLayer.Extensions;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.External.Por;
using Gdc.Scd.DataAccessLayer.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Import
{
    public class PorSwFspCodeTranslationService : ImportService<SwFspCodeTranslation>, ISwFspCodeTranslationService
    {
        private ILogger<LogLevel> _logger;

        public PorSwFspCodeTranslationService(IRepositorySet repositorySet, IEqualityComparer<SwFspCodeTranslation> comparer,
            ILogger<LogLevel> logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }


        public bool UploadSoftware(IEnumerable<SCD2_v_SAR_new_codes> softwareCodes,
            IEnumerable<Wg> warranties,
            IEnumerable<SwDigit> swDigits,
            IEnumerable<SCD2_SW_Overview> swInfo,
            Dictionary<string, long> availabilities, Dictionary<string, long> reactionTime,
            Dictionary<string, long> reactionTypes, Dictionary<string, long> locations,
            Dictionary<string, long> durations,
            DateTime createdDateTime, IEnumerable<string> softwareServiceType)
        {
            var result = true;
            var updatedFspCodes = new List<SwFspCodeTranslation>();

            try
            {
                foreach (var code in softwareCodes)
                {
                    if (String.IsNullOrEmpty(code.WG))
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.EMPTY_SOG_WG, code.Service_Code);
                        continue;
                    }

                    var wg = warranties.FirstOrDefault(w => w.Name == code.WG);
                    if (wg == null)
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOW_WG, code.Service_Code, code.WG);
                        continue;
                    }

                    var sla = code.MapFspCodeToSla(locations, durations, reactionTime, reactionTypes, availabilities);
                    if (sla == null)
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_SLA_TRANSLATION, code.Service_Code);
                        continue;
                    }

                    var swRecords = swInfo.Where(sw => sw.Service_Code.Equals(code.Service_Code)).ToList();
                    if (swRecords.Count != 1)
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.INCORRECT_SOFTWARE_FSPCODE_DIGIT_MAPPING, code.Service_Code, swRecords.Count);
                        continue;
                    }

                    var digit = swDigits.FirstOrDefault(d => d.Name == swRecords[0].Software_Lizenz_Digit);
                    if (digit == null)
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOW_DIGIT, code.Service_Code, swRecords[0].Software_Lizenz_Digit);
                        continue;
                    }

                    var dbcode = new SwFspCodeTranslation
                    {
                        AvailabilityId = sla.Availability,
                        DurationId = sla.Duration,
                        ReactionTimeId = sla.ReactionTime,
                        ReactionTypeId = sla.ReactionType,
                        ServiceLocationId = sla.ServiceLocation,
                        WgId = wg.Id,
                        Name = code.Service_Code,
                        SCD_ServiceType = code.SCD_ServiceType,
                        SecondSLA = code.SecondSLA,
                        ServiceDescription = code.SAP_Kurztext_Englisch,
                        EKSAPKey = code.EKSchluesselSAP,
                        EKKey = code.EKSchluessel,
                        Status = code.VStatus,
                        SwDigitId = digit.Id
                    };

                    updatedFspCodes.Add(dbcode);
                }

                var added = this.AddOrActivate(updatedFspCodes, createdDateTime);

                foreach (var addedEntity in added)
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(SwFspCodeTranslation), addedEntity.Name);
                }

                _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_END, added.Count);
            }

            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
            }

            return result;
        }


        public bool DeactivateFspSoftware(IEnumerable<SCD2_v_SAR_new_codes> fspCodes, DateTime modifiedDate)
        {
            var result = true;

            try
            {
                _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(SwFspCodeTranslation));

                var porItems = fspCodes.Select(code => code.Service_Code.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD
                var itemsToDeacivate = this.GetAll()
                                          .Where(s => !porItems.Contains(s.Name.ToLower())
                                                    && !s.DeactivatedDateTime.HasValue).ToList();

                var deactivated = this.Deactivate(itemsToDeacivate, modifiedDate);

                if (deactivated)
                {
                    foreach (var deactivateItem in itemsToDeacivate)
                    {
                        _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATED_ENTITY,
                            nameof(SwFspCodeTranslation), deactivateItem.Name);
                    }
                }

                _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATE_STEP_END, itemsToDeacivate.Count);
            }

            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
            }

            return result;
        }
    }
}
