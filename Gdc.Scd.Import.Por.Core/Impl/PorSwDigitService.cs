using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorSwDigitService : ImportService<SwDigit>, IPorSwDigitService
    {
        private ILogger _logger;

        public PorSwDigitService(IRepositorySet repositorySet,
            IEqualityComparer<SwDigit> comparer,
            ILogger logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool Deactivate(IDictionary<string, SCD2_SW_Overview> swInfo, DateTime modifiedDateTime)
        {
            var result = true;

            try
            {
                _logger.Info(PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(SwDigit));

                var porItems = swInfo.Keys.Select(k => k.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD
                var itemsToDeacivate = this.GetAll()
                                          .Where(f => !porItems.Contains(f.Name.ToLower())
                                                    && !f.DeactivatedDateTime.HasValue).ToList();

                var deactivated = this.Deactivate(itemsToDeacivate, modifiedDateTime);

                if (deactivated)
                {
                    foreach (var deactivateItem in itemsToDeacivate)
                    {
                        _logger.Debug(PorImportLoggingMessage.DEACTIVATED_ENTITY,
                            nameof(SwDigit), deactivateItem.Name);
                    }
                }

                _logger.Info(PorImportLoggingMessage.DEACTIVATE_STEP_END, itemsToDeacivate.Count);
            }

            catch (Exception ex)
            {
                _logger.Error(ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
            }

            return result;
        }
        public (bool, List<SwDigit>) UploadSwDigits(IDictionary<string, SCD2_SW_Overview> swInfo,
    IEnumerable<Sog> sogs,
    DateTime modifiedDateTime, List<UpdateQueryOption> updateOptions)
        {
            bool result = true;
            List<SwDigit> novice;

            try
            {
                _logger.Info(PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(SwDigit));

                var updatedSwDigits = new List<SwDigit>();

                foreach (var swDigit in swInfo)
                {
                    var sog = sogs.FirstOrDefault(p => p.Name.Equals(swDigit.Value.SOG_Code, StringComparison.OrdinalIgnoreCase));
                    if (sog == null)
                    {
                        _logger.Warn(
                            PorImportLoggingMessage.UNKNOWN_SOG, $"{nameof(SwDigit)} {swDigit.Key}", swDigit.Value.SOG_Code);
                        continue;
                    }

                    updatedSwDigits.Add(new SwDigit
                    {
                        Name = swDigit.Key,
                        SogId = sog.Id,
                        Sog = sog,
                        Description = swDigit.Value.Software_Lizenz_Beschreibung
                    });
                }

                var (added, inserted) = this.Add(updatedSwDigits, modifiedDateTime, updateOptions);

                foreach (var addedEntity in added)
                {
                    _logger.Debug(PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(SwDigit), addedEntity.Name);
                }

                novice = inserted;
                _logger.Info(PorImportLoggingMessage.ADD_STEP_END, added.Count);
            }

            catch (Exception ex)
            {
                _logger.Error(ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
                novice = null;
            }

            return (result, novice);
        }
    }
}

