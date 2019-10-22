using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Import.Por.Core.Dto;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class PorWgService : ImportService<Wg>, IPorWgService
    {
        private ILogger _logger;

        public PorWgService(IRepositorySet repositorySet, IEqualityComparer<Wg> comparer,
            ILogger logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public bool DeactivateWgs(IEnumerable<WgPorDto> wgs, DateTime modifiedDatetime)
        {
            var result = true;

            try
            {
                _logger.Info(PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(Wg));

                var porItems = wgs.Select(w => w.Name.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD and also either does not
                //exists in Logistic DB or was already deactivated there
                var itemsToDeacivate = this.GetAll()
                                          .Where(w => w.WgType == Scd.Core.Enums.WgType.Por && 
                                                      !porItems.Contains(w.Name.ToLower())
                                                    && !w.DeactivatedDateTime.HasValue 
                                                    && (!w.ExistsInLogisticsDb || w.IsDeactivatedInLogistic)).ToList();

                var deactivated = this.Deactivate(itemsToDeacivate, modifiedDatetime);

                if (deactivated)
                {
                    foreach (var deactivateItem in itemsToDeacivate)
                    {
                        _logger.Debug(PorImportLoggingMessage.DEACTIVATED_ENTITY,
                            nameof(Wg), deactivateItem.Name);
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

        public bool UploadWgs(IEnumerable<WgPorDto> wgs, 
            IEnumerable<Sog> sogs, 
            IEnumerable<Pla> plas, DateTime modifiedDateTime, 
            List<UpdateQueryOption> updateOptions)
        {
            var result = true;
            _logger.Info(PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(Wg));
            var updatedWgs = new List<Wg>();

            try
            {
                var defaultCentralContractGroup = this.repositorySet.GetRepository<CentralContractGroup>()
                                                    .GetAll().FirstOrDefault(ccg => ccg.Code == "NA");

                var defaultSFab = this.repositorySet.GetRepository<SFab>()
                                      .GetAll().FirstOrDefault(sf => sf.Name == "NA");

                foreach (var porWg in wgs)
                {
                    var pla = plas.FirstOrDefault(p => p.Name.Equals(porWg.Pla, StringComparison.OrdinalIgnoreCase));

                    if (pla == null)
                    {
                        _logger.Warn(PorImportLoggingMessage.UNKNOWN_PLA, $"{nameof(Wg)} {porWg.Name}", porWg.Pla);
                        continue;
                    }

                    Sog sog = sogs.FirstOrDefault(wg => wg.Name.Equals(porWg.Sog, StringComparison.OrdinalIgnoreCase));
                    if (sog == null && !String.IsNullOrEmpty(porWg.Sog))
                        _logger.Warn(PorImportLoggingMessage.SOG_NOT_EXISTS, porWg.Sog);

                    var newWg = new Wg
                    {
                        Alignment = porWg.Alignment,
                        Description = porWg.Description,
                        Name = porWg.Name,
                        PlaId = pla.Id,
                        SogId = sog?.Id,
                        CentralContractGroupId = defaultCentralContractGroup?.Id,
                        SFabId = defaultSFab?.Id,
                        FabGrp = porWg.FabGrp,
                        WgType = Scd.Core.Enums.WgType.Por,
                        ExistsInLogisticsDb = false,
                        IsDeactivatedInLogistic = false,
                        SCD_ServiceType = porWg.SCD_ServiceType,
                        IsSoftware = porWg.IsSoftware,
                        CompanyId = pla.CompanyId,
                        ServiceTypes = porWg.ServiceTypes
                    };

                    updatedWgs.Add(newWg);
                }

                var added = this.AddOrActivate(updatedWgs, modifiedDateTime, updateOptions);

                foreach (var addedEntity in added)
                {
                    _logger.Debug(PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(Wg), addedEntity.Name);
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
