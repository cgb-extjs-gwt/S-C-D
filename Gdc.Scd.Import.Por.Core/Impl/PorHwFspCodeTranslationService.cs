using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Extensions;
using Gdc.Scd.Import.Por.Core.Import;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorHwFspCodeTranslationService : PorFspTranslationService<TempHwFspCodeTranslation>, IHwFspCodeTranslationService<HwFspCodeDto>
    {
        private readonly ILogger _logger;

        public PorHwFspCodeTranslationService(IRepositorySet repositorySet,
            ILogger logger) : base(repositorySet)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool UploadHardware(HwFspCodeDto model)
        {

            try
            {
                _logger.Info(PorImportLoggingMessage.DELETE_BEGIN, nameof(TempHwFspCodeTranslation));
                _repository.DeleteAll();
                _logger.Info(PorImportLoggingMessage.DELETE_END);

                _logger.Info(PorImportLoggingMessage.UPLOAD_HW_CODES_START, "HW Codes");
                var hwResult = true;

                hwResult = UploadCodes(model.HardwareCodes, code => code.Country, model.HwSla, model.Sla,
                                       model.CreationDate, model.ProactiveServiceTypes, false);

                _logger.Info(PorImportLoggingMessage.UPLOAD_HW_CODES_ENDS, hwResult ? "0" : "-1");

                _logger.Info(PorImportLoggingMessage.UPLOAD_HW_CODES_START, "HW Codes: ProActive");


                var proActiveResult = UploadCodes(model.ProactiveCodes, code => code.Country, model.HwSla, model.Sla, model.CreationDate,
                                                  model.ProactiveServiceTypes, true);

                _logger.Info(PorImportLoggingMessage.UPLOAD_HW_CODES_ENDS, proActiveResult ? "0" : "-1");

                _logger.Info(PorImportLoggingMessage.UPLOAD_HW_CODES_START, "HW Codes: Standard Warranty");



                var stdwResult = UploadStdws(model.StandardWarranties,
                                                GetCountryFunc(model),
                                                model.HwSla, model.Sla,
                                                model.CreationDate);


                _logger.Info(PorImportLoggingMessage.UPLOAD_HW_CODES_ENDS, stdwResult ? "0" : "-1");

                var result = hwResult && proActiveResult && stdwResult;

                if (result)
                {
                    new CopyHwFspCodeTranslations(_repositorySet).Execute();
                }

                return result;
            }
            catch (Exception ex)
            {

                _logger.Error(ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                return false;
            }
        }

        private bool UploadStdws(IEnumerable<SCD2_v_SAR_new_codes> stdwCodes,
            Func<SCD2_v_SAR_new_codes, List<string>> getCountryCode,
            HwSlaDto stdwSla,
            SlaDictsDto slaDto,
            DateTime createdDateTime)
        {
            var result = true;
            var updatedFspCodes = new List<TempHwFspCodeTranslation>();
            try
            {
                foreach (var code in stdwCodes)
                {
                    List<long> wgs = new List<long>();

                    if (String.IsNullOrEmpty(code.WG) && String.IsNullOrEmpty(code.SOG))
                    {
                        _logger.Warn(PorImportLoggingMessage.EMPTY_SOG_WG, code.Service_Code);
                        continue;
                    }

                    //If FSP Code is binded to SOG
                    if (String.IsNullOrEmpty(code.WG))
                    {
                        var sog = stdwSla.Sogs.FirstOrDefault(s => s.Name == code.SOG);
                        if (sog == null)
                        {
                            _logger.Warn(PorImportLoggingMessage.UNKNOWN_SOG, code.Service_Code, code.SOG);
                            continue;
                        }

                        wgs.AddRange(stdwSla.Wgs.Where(w => w.SogId == sog.Id).Select(w => w.Id));
                    }

                    //FSP Code is binded to WG
                    else
                    {
                        var wg = stdwSla.Wgs.FirstOrDefault(w => w.Name == code.WG);
                        if (wg == null)
                        {
                            _logger.Warn(PorImportLoggingMessage.UNKNOW_WG, code.Service_Code, code.WG);
                            continue;
                        }

                        wgs.Add(wg.Id);
                    }

                    var sla = code.MapFspCodeToSla(slaDto, stdwSla.Proactive);

                    if (sla == null)
                    {
                        _logger.Warn(PorImportLoggingMessage.UNKNOWN_SLA_TRANSLATION, code.Service_Code);
                        continue;
                    }

                    var countryCodes = getCountryCode(code);

                    //if there are no records in LUT table for FSP code -> add nothing
                    if (countryCodes.Count == 0)
                    {
                        continue;
                    }

                    foreach (var countryCode in countryCodes)
                    {
                        //if Country Group is unknown or empty than skip it
                        if (String.IsNullOrEmpty(countryCode) ||
                            !stdwSla.Countries.ContainsKey(countryCode))
                        {
                            _logger.Warn(PorImportLoggingMessage.UNKNOWN_COUNTRY_DIGIT,
                                code.Service_Code, countryCode);
                        }

                        else
                        {
                            foreach (var country in stdwSla.Countries[countryCode])
                            {
                                foreach (var wg in wgs)
                                {
                                    var dbcode = AddStdwCode(sla, country, wg, code, createdDateTime, countryCode);
                                    _logger.Debug(PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                                                    nameof(TempHwFspCodeTranslation), dbcode.Name);

                                    updatedFspCodes.Add(dbcode);
                                }
                            }
                        }
                    }
                }

                this.Save(updatedFspCodes);

                _logger.Info(PorImportLoggingMessage.ADD_STEP_END, updatedFspCodes.Count);
                return result;
            }

            catch (Exception ex)
            {
                _logger.Error(ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                return false;
            }
        }

        private bool UploadCodes(IEnumerable<SCD2_v_SAR_new_codes> hardwareCodes,
            Func<SCD2_v_SAR_new_codes, string> getCountryCode,
            HwSlaDto hwSla,
            SlaDictsDto slaDto,
            DateTime createdDateTime,
            IEnumerable<string> proactiveServiceType,
            bool isProactive)
        {
            var result = true;

            var updatedFspCodes = new List<TempHwFspCodeTranslation>();

            try
            {
                foreach (var code in hardwareCodes)
                {
                    //map country
                    var countryCode = getCountryCode(code);

                    if (String.IsNullOrEmpty(countryCode) || !hwSla.Countries.ContainsKey(countryCode))
                    {
                        _logger.Warn(PorImportLoggingMessage.UNKNOWN_COUNTRY_DIGIT, code.Service_Code, countryCode);
                        continue;
                    }

                    //map warranty groups
                    var wgs = code.MapFspCodeToWgs(hwSla.Wgs, hwSla.Sogs, _logger);

                    if (!wgs.Any())
                        continue;

                    //map sla
                    var sla = isProactive ? code.MapFspCodeToSla(slaDto, hwSla.Proactive, true, proactiveServiceType) :
                                            code.MapFspCodeToSla(slaDto, hwSla.Proactive);

                    if (sla == null)
                    {
                        _logger.Warn(PorImportLoggingMessage.UNKNOWN_SLA_TRANSLATION, code.Service_Code);
                        continue;
                    }

                    foreach (var country in hwSla.Countries[countryCode])
                    {
                        foreach (var wg in wgs)
                        {
                            var dbcode = new TempHwFspCodeTranslation
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
                                ProactiveSlaId = sla.ProActive,
                                ServiceType = code.ServiceType,
                                CreatedDateTime = createdDateTime,
                                IsStandardWarranty = false,
                                SapItemCategory = code.Item_Category
                            };

                            _logger.Debug(PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                                            nameof(TempHwFspCodeTranslation), dbcode.Name);

                            updatedFspCodes.Add(dbcode);
                        }
                    }

                }

                this.Save(updatedFspCodes);

                _logger.Info(PorImportLoggingMessage.ADD_STEP_END, updatedFspCodes.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                return false;
            }
        }


        private TempHwFspCodeTranslation AddStdwCode(SlaDto sla, long? country,
            long wg, SCD2_v_SAR_new_codes code,
            DateTime createdDateTime, string lutCode)
        {
            var dbcode = new TempHwFspCodeTranslation
            {
                AvailabilityId = sla.Availability,
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
                ProactiveSlaId = sla.ProActive,
                ServiceType = code.ServiceType,
                CreatedDateTime = createdDateTime,
                IsStandardWarranty = true,
                LUT = lutCode,
                SapItemCategory = code.Item_Category
            };

            if (country.HasValue)
                dbcode.CountryId = country.Value;

            return dbcode;
        }

        private Func<SCD2_v_SAR_new_codes, List<string>> GetCountryFunc(HwFspCodeDto model)
        {
            return code =>
            {
                var mapping = model.LutCodes.Where(c => c.Service_Code.Equals(code.Service_Code))
                                   .ToList();

                if (mapping.Count == 0)
                    return new List<string>();


                return mapping.Select(lut => lut.Country_Group).ToList();
            };
        }


    }
}
