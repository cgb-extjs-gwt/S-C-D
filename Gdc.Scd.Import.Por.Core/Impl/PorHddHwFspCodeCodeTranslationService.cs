using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Extensions;
using Gdc.Scd.Import.Por.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorHddHwFspCodeCodeTranslationService : PorFspTranslationService<HwHddFspCodeTranslation>,
        IHwFspCodeTranslationService<HwHddFspCodeDto>
    {
        private readonly ILogger<LogLevel> _logger;

        public PorHddHwFspCodeCodeTranslationService(IRepositorySet repositorySet,
            ILogger<LogLevel> logger) : base(repositorySet)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool UploadHardware(HwHddFspCodeDto model)
        {
            using (var transaction = this._repositorySet.GetTransaction())
            {
                try
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.DELETE_BEGIN, nameof(HwHddFspCodeTranslation));
                    _repository.DeleteAll();
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.DELETE_END);

                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_START, "HW HDD Codes");
                    var result = true;

                    result = UploadCodes(model.HardwareCodes, model.HwSla, model.CreationDate);
                    transaction.Commit();
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.UPLOAD_HW_CODES_ENDS, result ? "0" : "-1");
                    return result;
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                    return false;
                }
             }
        }

        public bool UploadCodes(IEnumerable<SCD2_v_SAR_new_codes> hddCodes,
            HwSlaDto hwSla,
            DateTime createdDateTime)
        {
            var result = true;

            var updatedFspCodes = new List<HwHddFspCodeTranslation>();
            try
            {
                foreach(var code in hddCodes)
                {
                    //map country
                    var countryCode = code.Country;
                    if (String.IsNullOrEmpty(countryCode) || !hwSla.Countries.ContainsKey(countryCode))
                    {
                        _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_COUNTRY_DIGIT, code.Service_Code, countryCode);
                        continue;
                    }

                    //map warranty groups
                    var wgs = code.MapFspCodeToWgs(hwSla.Wgs, hwSla.Sogs, _logger);

                    if (!wgs.Any())
                        continue;

                    foreach (var country in hwSla.Countries[countryCode])
                    {
                        foreach (var wg in wgs)
                        {
                            var dbcode = new HwHddFspCodeTranslation
                            {
                                CountryId = country,
                                WgId = wg,
                                Name = code.Service_Code,
                                SCD_ServiceType = code.SCD_ServiceType,
                                SecondSLA = code.SecondSLA,
                                ServiceDescription = code.SAP_Kurztext_Englisch,
                                EKSAPKey = code.EKSchluesselSAP,
                                EKKey = code.EKSchluessel,
                                Status = code.VStatus,
                                CreatedDateTime = createdDateTime,
                            };

                            _logger.Log(LogLevel.Debug, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                                            nameof(HwHddFspCodeTranslation), dbcode.Name);

                            updatedFspCodes.Add(dbcode);
                        }
                    }
                }

                this.Save(updatedFspCodes);

                _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_END, updatedFspCodes.Count);
                return result;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
