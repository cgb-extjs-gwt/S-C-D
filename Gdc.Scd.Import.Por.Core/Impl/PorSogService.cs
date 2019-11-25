using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorSogService : ImportService<Sog>, IPorSogService
    {
        private ILogger _logger;

        public PorSogService(IRepositorySet repositorySet, IEqualityComparer<Sog> comparer,
            ILogger logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }


        public bool DeactivateSogs(IEnumerable<SogPorDto> sogs, DateTime modifiedDatetime)
        {
            var result = true;

            try
            {
                _logger.Info(PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(Sog));

                var porItems = sogs.Select(s => s.Name.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD
                var itemsToDeacivate = this.GetAll()
                                          .Where(s => !porItems.Contains(s.Name.ToLower())
                                                    && !s.DeactivatedDateTime.HasValue).ToList();

                var deactivated = this.Deactivate(itemsToDeacivate, modifiedDatetime);

                if (deactivated)
                {
                    foreach (var deactivateItem in itemsToDeacivate)
                    {
                        _logger.Debug(PorImportLoggingMessage.DEACTIVATED_ENTITY,
                            nameof(Sog), deactivateItem.Name);
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

        public bool UploadSogs(IEnumerable<SogPorDto> sogs,
            IEnumerable<Pla> plas,
            DateTime modifiedDateTime,
            List<UpdateQueryOption> updateOptions)
        {
            var result = true;
            _logger.Info(PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(Sog));
            var updatedSogs = new List<Sog>();

            try
            {
                var defaultSFab = this.repositorySet.GetRepository<SFab>()
                                      .GetAll().FirstOrDefault(sf => sf.Name == "NA");

                foreach (var porSog in sogs)
                {
                    var pla = plas.FirstOrDefault(p => p.Name.Equals(porSog.Pla, StringComparison.OrdinalIgnoreCase));

                    if (pla == null)
                    {
                        _logger.Warn(PorImportLoggingMessage.UNKNOWN_PLA, $"{nameof(Sog)} {porSog.Name}", porSog.Pla);
                        continue;
                    }

                    var newSog = new Sog
                    {
                        Alignment = porSog.Alignment,
                        Description = porSog.Description,
                        Name = porSog.Name,
                        PlaId = pla.Id,
                        FabGrp = porSog.FabGrp,
                        SCD_ServiceType = porSog.SCD_ServiceType,
                        SFabId = defaultSFab?.Id,
                        IsSoftware = porSog.IsSoftware,
                        IsSolution = porSog.IsSolution,
                        ServiceTypes = porSog.ServiceTypes
                    };

                    updatedSogs.Add(newSog);
                }

                var added = this.AddOrActivate(updatedSogs, modifiedDateTime, updateOptions);

                foreach (var addedEntity in added)
                {
                    _logger.Debug(PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(Sog), addedEntity.Name);
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
