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
    public class PorWgService : ImportService<Wg>, IPorWgService
    {
        private ILogger<LogLevel> _logger;

        public PorWgService(IRepositorySet repositorySet, IEqualityComparer<Wg> comparer,
            ILogger<LogLevel> logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool DeactivateSogs(IEnumerable<SCD2_WarrantyGroups> wgs, DateTime modifiedDatetime)
        {
            var result = true;

            try
            {
                _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(Wg));

                var porItems = wgs.Select(w => w.Warranty_Group.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD
                var itemsToDeacivate = this.GetAll()
                                          .Where(w => !porItems.Contains(w.Name.ToLower())
                                                    && !w.DeactivatedDateTime.HasValue).ToList();

                var deactivated = this.Deactivate(itemsToDeacivate, modifiedDatetime);

                if (deactivated)
                {
                    foreach (var deactivateItem in itemsToDeacivate)
                    {
                        _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATED_ENTITY,
                            nameof(Wg), deactivateItem.Name);
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

        public bool UploadWgs(IEnumerable<SCD2_WarrantyGroups> wgs, 
            IEnumerable<SFab> sFabs, IEnumerable<Sog> sogs, 
            IEnumerable<Pla> plas, DateTime modifiedDateTime)
        {
            var result = true;
            _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(Wg));
            var updatedWgs = new List<Wg>();

            try
            {
                foreach (var porWg in wgs)
                {
                    var pla = plas.FirstOrDefault(p => p.Name.Equals(porWg.Warranty_PLA, StringComparison.OrdinalIgnoreCase));

                    if (pla == null)
                    {
                        _logger.Log(LogLevel.Warn,
                               PorImportLoggingMessage.UNKNOWN_PLA, $"{nameof(Wg)} {porWg.Warranty_Group}", porWg.Warranty_PLA);
                        continue;
                    }

                    SFab sFab = sFabs.FirstOrDefault(fab =>
                                fab.Name.Equals(porWg.FabGrp, StringComparison.OrdinalIgnoreCase));

                    Sog sog = sogs.FirstOrDefault(wg => wg.Name.Equals(porWg.SOG, StringComparison.OrdinalIgnoreCase));

                    updatedWgs.Add(new Wg
                    {
                        Alignment = porWg.Alignment,
                        Description = porWg.Warranty_Group_Name,
                        Name = porWg.Warranty_Group,
                        PlaId = pla.Id,
                        SFabId = sFab?.Id,
                        SogId = sog?.Id
                    });
                }

                var added = this.AddOrActivate(updatedWgs, modifiedDateTime);

                foreach (var addedEntity in added)
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(Wg), addedEntity.Name);
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
