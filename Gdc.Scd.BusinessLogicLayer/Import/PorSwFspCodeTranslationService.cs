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
    public class PorSwFspCodeTranslationService : PorFspTranslationService<SwFspCodeTranslation>, ISwFspCodeTranslationService
    {
        private readonly ILogger<LogLevel> _logger;

        public PorSwFspCodeTranslationService(IRepositorySet repositorySet,
            ILogger<LogLevel> logger) : base(repositorySet)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }


        public bool UploadSoftware(IEnumerable<SCD2_v_SAR_new_codes> softwareCodes,
            IEnumerable<Sog> sogs,
            IEnumerable<SwDigit> swDigits,
            IEnumerable<SCD2_SW_Overview> swInfo,
            Dictionary<string, long> availabilities, Dictionary<string, long> reactionTime,
            Dictionary<string, long> reactionTypes, Dictionary<string, long> locations,
            Dictionary<string, long> durations,
            DateTime createdDateTime, IEnumerable<string> softwareServiceType)
        {
            var result = true;
            var updatedFspCodes = new List<SwFspCodeTranslation>();

            using (var transaction = this._repositorySet.GetTransaction())
            {
                try
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.DELETE_BEGIN, nameof(SwFspCodeTranslation));
                    _repository.DeleteAll();
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.DELETE_END);

                    foreach (var code in softwareCodes)
                    {
                        if (String.IsNullOrEmpty(code.SOG))
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.EMPTY_SOG_WG, code.Service_Code);
                            continue;
                        }

                        var sog = sogs.FirstOrDefault(s => s.Name == code.SOG);
                        if (sog == null)
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_SOG, code.Service_Code, code.SOG);
                            continue;
                        }

                        var sla = code.MapFspCodeToSla(locations, durations, reactionTime, reactionTypes, availabilities);
                        if (sla == null)
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_SLA_TRANSLATION, code.Service_Code);
                            continue;
                        }

                        var swRecords = swInfo.Where(sw => sw.Service_Code.Equals(code.Service_Code)).ToList();
                        var distinctDigits = swRecords.Select(r => r.Software_Lizenz_Digit).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                        if (distinctDigits.Count != 1)
                        { 
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.INCORRECT_SOFTWARE_FSPCODE_DIGIT_MAPPING, code.Service_Code, swRecords.Count);
                            continue;
                        }

                        var digit = swDigits.FirstOrDefault(d => d.Name == distinctDigits[0]);
                        if (digit == null)
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOW_DIGIT, code.Service_Code, distinctDigits[0]);
                            continue;
                        }

                        var dbcode = new SwFspCodeTranslation
                        {
                            AvailabilityId = sla.Availability,
                            DurationId = sla.Duration,
                            ReactionTimeId = sla.ReactionTime,
                            ReactionTypeId = sla.ReactionType,
                            ServiceLocationId = sla.ServiceLocation,
                            SogId = sog.Id,
                            Name = code.Service_Code,
                            SCD_ServiceType = code.SCD_ServiceType,
                            SecondSLA = code.SecondSLA,
                            ServiceDescription = code.SAP_Kurztext_Englisch,
                            EKSAPKey = code.EKSchluesselSAP,
                            EKKey = code.EKSchluessel,
                            Status = code.VStatus,
                            SwDigitId = digit.Id,
                            CreatedDateTime = createdDateTime
                        };

                        _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                                            nameof(SwFspCodeTranslation), dbcode.Name);

                        updatedFspCodes.Add(dbcode);
                    }


                    this.Save(updatedFspCodes);
                    transaction.Commit();

                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_END, updatedFspCodes.Count);
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
}
