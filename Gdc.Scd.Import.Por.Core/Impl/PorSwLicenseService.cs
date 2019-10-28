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
    public class PorSwLicenseService : ImportService<SwLicense>, IPorSwLicenseService
    {
        private ILogger _logger;

        public PorSwLicenseService(IRepositorySet repositorySet,
            IEqualityComparer<SwLicense> comparer,
            ILogger logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool Deactivate(IEnumerable<SCD2_SW_Overview> swInfo, DateTime modifiedDateTime)
        {
            var result = true;

            try
            {
                _logger.Info(PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(SwLicense));

                var porItems = swInfo.Select(sw => sw.Software_Lizenz.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD
                var itemsToDeacivate = this.GetAll()
                                          .Where(s => !porItems.Contains(s.Name.ToLower())
                                                    && !s.DeactivatedDateTime.HasValue).ToList();

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

        public bool UploadSwLicense(IEnumerable<SCD2_SW_Overview> swInfo,
            DateTime modifiedDateTime, List<UpdateQueryOption> updateOptions)
        {
            var result = true;

            try
            {
                _logger.Info(PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(SwLicense));

                var updatedSwLicenses = new List<SwLicense>();

                foreach (var swLicense in swInfo)
                {

                    updatedSwLicenses.Add(new SwLicense
                    {
                        Name = swLicense.Software_Lizenz,
                        Description = swLicense.Software_Lizenz_Benennung
                    });
                }

                var added = this.AddOrActivate(updatedSwLicenses, modifiedDateTime, updateOptions);

                foreach (var addedEntity in added)
                {
                    _logger.Debug(PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(SwLicense), addedEntity.Name);
                }

                _logger.Info(PorImportLoggingMessage.ADD_STEP_END, added.Count);
            }

            catch (Exception ex)
            {
                _logger.Error(ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
            }

            return result;
        }
    }
}
