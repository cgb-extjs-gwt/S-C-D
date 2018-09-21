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
    public class FspCodeTranslationService : ImportPorService<FspCodeTranslation>, IFspCodeTranslationService
    {
        private ILogger<LogLevel> _logger;

        public FspCodeTranslationService(IRepositorySet repositorySet, 
            IEqualityComparer<FspCodeTranslation> comparer,
            ILogger<LogLevel> logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool DeactivateFspCodes(IEnumerable<v_SAR_new_codes> fspCodes, 
            DateTime modifiedDatetime)
        {
            throw new NotImplementedException();
        }

        public bool UploadFspCodes(IEnumerable<v_SAR_new_codes> fspCodes,
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
            DateTime modifiedDate)
        {
            var result = true;

            _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(FspCodeTranslation));

            var updatedFspCodes = new List<FspCodeTranslation>();
            var serviceCodes = fspCodes.Where(code => otherServiceTypes.Contains(code.SCD_ServiceType));
            var proactiveCodes = fspCodes.Where(code => allowedProactiveTypes.Contains(code.SCD_ServiceType));
            var standardWarranties = fspCodes.Where(code => allowedStandardWarranties.Contains(code.SCD_ServiceType)
                                                        && code.SCD_ServiceType.Substring(11, 4).ToUpper() == "STDW");
            try
            {
                foreach (var fspCode in serviceCodes)
                {


                   
                }
            }

            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
            }

            return result;
            throw new NotImplementedException();
        }


        private FspCodeTranslation MapPorTranslationToScd(v_SAR_new_codes porFspCode, 
            IEnumerable<ServiceLocation> serviceLocations,
            IEnumerable<Duration> durations,
            IEnumerable<ReactionTime> reactionTimeValues,
            IEnumerable<ReactionType> reactionTypeValues,
            IEnumerable<Availability> availabilityValues)
        {
            return null;
        }
    }
}
