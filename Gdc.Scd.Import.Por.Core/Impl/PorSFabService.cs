using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorSFabService : ImportService<SFab>, IPorSFabsService
    {
        private ILogger<LogLevel> _logger;

        public PorSFabService(IRepositorySet repositorySet, IEqualityComparer<SFab> comparer, 
            ILogger<LogLevel> logger) 
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            
            _logger = logger;
        }

        public bool DeactivateSFabs(IDictionary<string, string> porFabsDictionary, DateTime modifiedDate)
        {
            var result = true;

            try
            {
                _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(SFab));

                var porItems = porFabsDictionary.Keys.Select(k => k.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD
                var itemsToDeacivate = this.GetAll()
                                          .Where(f => !porItems.Contains(f.Name.ToLower())
                                                    && !f.DeactivatedDateTime.HasValue).ToList();

                var deactivated = this.Deactivate(itemsToDeacivate, modifiedDate);

                if (deactivated)
                {
                    foreach (var deactivateItem in itemsToDeacivate)
                    {
                        _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATED_ENTITY,
                            nameof(SFab), deactivateItem.Name);
                    }
                }

                _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATE_STEP_END, itemsToDeacivate.Count);
            }

            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
            }

            return result;
        }

        public bool UploadSFabs(IDictionary<string, string> porFabsDictionary,
                                IEnumerable<Pla> plas, DateTime modifiedDateTime)
        {
            var result = true;

            try
            {
                _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(SFab));
                var updatedSFabs = new List<SFab>();

                foreach (var fab in porFabsDictionary)
                {
                    var pla = plas.FirstOrDefault(p => p.Name.Equals(fab.Value, StringComparison.OrdinalIgnoreCase));
                    if (pla == null)
                    {
                        _logger.Log(LogLevel.Warn,
                            PorImportLoggingMessage.UNKNOWN_PLA, $"{nameof(SFab)} {fab.Key}", fab.Value);
                        continue;
                    }
                    updatedSFabs.Add(new SFab
                    {
                        Name = fab.Key,
                        PlaId = pla.Id
                    });                    
                }

                var added = this.AddOrActivate(updatedSFabs, modifiedDateTime);

                foreach (var addedEntity in added)
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY, 
                        nameof(SFab), addedEntity.Name);
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
