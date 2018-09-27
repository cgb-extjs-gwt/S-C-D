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
    public class PorSwDigitLicenseService : IPorSwDigitLicenseService
    {
        private readonly ILogger<LogLevel> _logger;
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<SwDigitLicense> _repository;

        public PorSwDigitLicenseService(IRepositorySet repositorySet,
            ILogger<LogLevel> logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            _logger = logger;
            _repositorySet = repositorySet;
            _repository = _repositorySet.GetRepository<SwDigitLicense>();
        }

        public bool UploadSwDigitAndLicenseRelation(IEnumerable<SwLicense> licenses, 
            IEnumerable<SwDigit> digits, 
            IEnumerable<SCD2_SW_Overview> swInfo, DateTime created)
        {
            bool result = true;
            using (var transaction = this._repositorySet.GetTransaction())
            {
                try
                {
                    _repository.DeleteAll();
                    var combinations = new List<SwDigitLicense>();

                    foreach (var swCombination in swInfo)
                    {
                        var digit = digits.FirstOrDefault(d => d.Name.Equals(swCombination.Software_Lizenz_Digit, StringComparison.OrdinalIgnoreCase));
                        if (digit == null)
                        {
                            this._logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOW_DIGIT, nameof(SwDigitLicense), swCombination.Software_Lizenz_Digit);
                            continue;
                        }
                        var license = licenses.FirstOrDefault(l => l.Name.Equals(swCombination.Software_Lizenz, StringComparison.OrdinalIgnoreCase));
                        if (license == null)
                        {
                            this._logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_LICENSE, nameof(SwDigitLicense), swCombination.Software_Lizenz);
                            continue;
                        }
                        var newCombination = combinations.FirstOrDefault(swd => swd.SwDigitId == digit.Id && swd.SwLicenseId == license.Id);
                        if (newCombination == null)
                        {
                            _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADDING_SWDIGIT_SWLICENSE, license.Name, digit.Name);
                            combinations.Add(new SwDigitLicense { SwDigitId = digit.Id, SwLicenseId = license.Id, CreatedDateTime = created });
                        }
                    }

                    _repository.Save(combinations);
                    _repositorySet.Sync();
                    transaction.Commit();
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_END, combinations.Count);
                }

                catch (Exception ex)
                {
                    result = false;
                    transaction.Rollback();
                    _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                }
            }

            return result;
        }
    }
}
