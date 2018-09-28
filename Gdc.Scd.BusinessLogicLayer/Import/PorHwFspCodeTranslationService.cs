using Gdc.Scd.BusinessLogicLayer.Extensions;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.CapabilityMatrix;
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
    public class PorHwFspCodeTranslationService : ImportService<HwFspCodeTranslation>, IHwFspCodeTranslationService
    {
        private ILogger<LogLevel> _logger;

        public PorHwFspCodeTranslationService(IRepositorySet repositorySet, IEqualityComparer<HwFspCodeTranslation> comparer,
            ILogger<LogLevel> logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool UploadHardware(
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
            IEnumerable<string> otherHardware)
        {
            _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_START, "HW Codes");
            var hwResult = true;

            hwResult = UploadCodes(hardwareCodes, code => code.Country, countries, warranties, sogs, availabilities,
                                    reactionTime, reactionTypes, locations, durations, proActive, matrix, createdDateTime, proActiveServiceTypes, false);

            _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_ENDS, hwResult ? "0" : "-1");

            _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_START, "HW Codes: ProActive");
            
                
            var proActiveResult =  UploadCodes(proActiveCodes, code => code.Country, countries, warranties, sogs, availabilities,
                                 reactionTime, reactionTypes, locations, durations, proActive, matrix, createdDateTime, proActiveServiceTypes, true);

            _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_ENDS, proActiveResult ? "0" : "-1");

            _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_START, "HW Codes: Standard Warranty");

            Func<SCD2_v_SAR_new_codes, string> getCountryCode = code =>
            {
                var mapping = stdw.FirstOrDefault(c => c.Service_Code.Equals(code));
                if (mapping == null)
                    return null;
                return mapping.Country_Group;
            };

            var stdwResult = UploadCodes(stdwCodes, getCountryCode, countries, warranties, sogs, availabilities,
                                    reactionTime, reactionTypes, locations, durations, proActive, matrix, createdDateTime, proActiveServiceTypes, false);

            _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_ENDS, stdwResult ? "0" : "-1");

            return hwResult && proActiveResult && stdwResult;
        }

        

        public bool DeactivateFspHardware(IEnumerable<SCD2_v_SAR_new_codes> fspCodes, DateTime modifiedDate)
        {
            var result = true;

            try
            {
                _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(HwFspCodeTranslation));

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
                            nameof(HwFspCodeTranslation), deactivateItem.Name);
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

        private long? MapSlaToMatrix(HwFspCodeTranslation code, IQueryable<CapabilityMatrix> matrix)
        {
            var record = matrix.FirstOrDefault(m => m.Availability.Id == code.AvailabilityId &&
                                                    m.Duration.Id == code.DurationId &&
                                                    m.ReactionTime.Id == code.ReactionTimeId &&
                                                    m.ReactionType.Id == code.ReactionTypeId &&
                                                    m.ServiceLocation.Id == code.ServiceLocationId &&
                                                    m.Wg.Id == code.WgId &&
                                                    m.Country.Id == code.CountryId);
            return record?.Id;
        }


        private bool UploadCodes (IEnumerable<SCD2_v_SAR_new_codes> hardwareCodes,
            Func<SCD2_v_SAR_new_codes, string> getCountryCode,
            Dictionary<string, List<long>> countries,
            IEnumerable<Wg> warranties,
            IEnumerable<Sog> sogs,
            Dictionary<string, long> availabilities, Dictionary<string, long> reactionTime,
            Dictionary<string, long> reactionTypes, Dictionary<string, long> locations,
            Dictionary<string, long> durations, Dictionary<string, long> proActive,
            IQueryable<CapabilityMatrix> matrix,
            DateTime createdDateTime,
            IEnumerable<string> proactiveServiceType,
            bool isProactive)
        {
            var result = true;

            var updatedFspCodes = new List<HwFspCodeTranslation>();

            try
            {
                foreach (var code in hardwareCodes)
                {
                    var countryCode = getCountryCode(code);

                    if (String.IsNullOrEmpty(countryCode) || !countries.ContainsKey(countryCode))
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_COUNTRY_DIGIT, code.Service_Code, countryCode);
                        continue;
                    }

                    List<long> wgs = new List<long>();

                    if (String.IsNullOrEmpty(code.WG) && String.IsNullOrEmpty(code.SOG))
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.EMPTY_SOG_WG, code.Service_Code);
                        continue;
                    }

                    //If FSP Code is binded to SOG
                    if (String.IsNullOrEmpty(code.WG))
                    {
                        var sog = sogs.FirstOrDefault(s => s.Name == code.SOG);
                        if (sog == null)
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOW_SOG, code.Service_Code, code.SOG);
                            continue;
                        }

                        wgs.AddRange(warranties.Where(w => w.SogId == sog.Id).Select(w => w.Id));
                    }

                    //FSP Code is binded to WG
                    else
                    {
                        var wg = warranties.FirstOrDefault(w => w.Name == code.WG);
                        if (wg == null)
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOW_WG, code.Service_Code, code.WG);
                            continue;
                        }

                        wgs.Add(wg.Id);
                    }

                    var sla = isProactive ? code.MapFspCodeToSla(locations, durations, reactionTime, reactionTypes, availabilities,
                                            proActive, true, proactiveServiceType) :
                                            code.MapFspCodeToSla(locations, durations, reactionTime, reactionTypes, availabilities);

                    if (sla == null)
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_SLA_TRANSLATION, code.Service_Code);
                        continue;
                    }

                    foreach (var country in countries[countryCode])
                    {
                        foreach (var wg in wgs)
                        {
                            var dbcode = new HwFspCodeTranslation
                            {
                                AvailabilityId = sla.Availability,
                                CountryId = country,
                                DurationId = sla.Duration,
                                ReactionTimeId = sla.ReactionTime,
                                ReactionTypeId = sla.ReactionType,
                                ServiceLocationId = sla.ServiceLocation,
                                WgId = wg,
                                Name = code.Service_Code,
                                SCD_ServiceType = code.SCD_ServiceType,
                                SecondSLA = code.SecondSLA,
                                ServiceDescription = code.SAP_Kurztext_Englisch,
                                EKSAPKey = code.EKSchluesselSAP,
                                EKKey = code.EKSchluessel,
                                Status = code.VStatus,
                                ProactiveSlaId = sla.ProActive
                            };

                            if (!isProactive)
                            {
                                var matrixId = MapSlaToMatrix(dbcode, matrix);

                                if (matrixId.HasValue)
                                    dbcode.MatrixId = matrixId;
                            }

                            updatedFspCodes.Add(dbcode);
                        }
                    }

                }

                var added = this.AddOrActivate(updatedFspCodes, createdDateTime);

                foreach (var addedEntity in added)
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(HwFspCodeTranslation), addedEntity.Name);
                }

                _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_END, added.Count);
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
