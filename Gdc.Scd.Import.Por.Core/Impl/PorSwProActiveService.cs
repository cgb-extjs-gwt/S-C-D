using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorSwProActiveService : IPorSwProActiveService
    {
        private ILogger<LogLevel> _logger;
        private IRepositorySet _repositorySet;
        private IRepository<ProActiveDigit> _swProActiveRepository;

        public PorSwProActiveService(IRepositorySet repositorySet,
            ILogger<LogLevel> logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            _logger = logger;
            _repositorySet = repositorySet;
            _swProActiveRepository = _repositorySet.GetRepository<ProActiveDigit>();
        }

        public bool UploadSwProactiveInfo(SwProActiveDto model)
        {
            bool result = true;
            using (var transaction = this._repositorySet.GetTransaction())
            {
                try
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.DELETE_BEGIN, nameof(ProActiveDigit));
                    _swProActiveRepository.DeleteAll();
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.DELETE_END);
                    var combinations = new List<ProActiveDigit>();
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(ProActiveDigit));
                    foreach (var proActiveDigit in model.ProActiveInfo)
                    {
                        var digit = model.SwDigits.FirstOrDefault(d => d.Name.Equals(proActiveDigit.ID));
                        if (digit == null)
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOW_DIGIT, nameof(ProActiveDigit), proActiveDigit.ID);
                            continue;
                        }

                        if(!model.Proactive.ContainsKey(proActiveDigit.Servicelevel))
                        {
                            _logger.Log(LogLevel.Warn, PorImportLoggingMessage.UNKNOWN_PROACTIVE, nameof(ProActiveDigit), proActiveDigit.Servicelevel);
                            continue;
                        }

                        var proActiveDigitDb = new ProActiveDigit
                        {
                            Name = proActiveDigit.ID,
                            Description = proActiveDigit.Beschreibung,
                            DigitId = digit.Id,
                            ProActiveId = model.Proactive[proActiveDigit.Servicelevel],
                            ProActive = proActiveDigit.Proactive,
                            ReactiveMappingDigit = proActiveDigit.SW_level_digit_ReactiveMapping,
                            CreatedDateTime = model.CreatedDateTime
                        };

                        _logger.Log(LogLevel.Debug, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY, nameof(ProActiveDigit), proActiveDigit.ID);
                        combinations.Add(proActiveDigitDb);
                    }

                    _swProActiveRepository.Save(combinations);
                    _repositorySet.Sync();
                    transaction.Commit();

                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_END, combinations.Count);
                }
                catch(Exception ex)
                {
                    _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                    result = false;
                }
            }
            return result;        
        }
    }
}
