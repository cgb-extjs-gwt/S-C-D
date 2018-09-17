using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.enums;
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
    public class PorSogService : ImportPorService<Sog>, IPorSogService
    {
        private ILogger<LogLevel> _logger;

        public PorSogService(IRepositorySet repositorySet, IEqualityComparer<Sog> comparer,
            ILogger<LogLevel> logger)
            : base(repositorySet, comparer)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }


        public bool DeactivateSogs(IEnumerable<Intranet_SOG_Info> sogs, DateTime modifiedDatetime)
        {
            var result = true;

            try
            {
                _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATE_STEP_BEGIN, nameof(Sog));

                var porItems = sogs.Select(s => s.SOG_Code.ToLower()).ToList();

                //select all that is not coming from POR and was not already deactivated in SCD
                var itemsToDeacivate = this.GetAll()
                                          .Where(s => !porItems.Contains(s.Name.ToLower())
                                                    && !s.DeactivatedDateTime.HasValue).ToList();

                var deactivated = this.Deactivate(itemsToDeacivate, modifiedDatetime);

                if (deactivated)
                {
                    foreach (var deactivateItem in itemsToDeacivate)
                    {
                        _logger.Log(LogLevel.Info, PorImportLoggingMessage.DEACTIVATED_ENTITY,
                            nameof(Sog), deactivateItem.Name);
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

        public bool UploadSogs(IEnumerable<Intranet_SOG_Info> sogs, 
            IEnumerable<Pla> plas,
            IEnumerable<SFab> sFabs,
            DateTime modifiedDateTime)
        {
            var result = true;
            _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_BEGIN, nameof(Sog));
            var updatedSogs = new List<Sog>();

            try
            {
                foreach (var porSog in sogs)
                {
                    var pla = plas.FirstOrDefault(p => p.Name.Equals(porSog.Produktreihe, StringComparison.OrdinalIgnoreCase));

                    if (pla == null)
                    {
                        _logger.Log(LogLevel.Warn,
                               PorImportLoggingMessage.UNKNOWN_PLA, $"{nameof(Sog)} {porSog.SOG_Code}", porSog.Produktreihe);
                        continue;
                    }

                    SFab sFab = sFabs.FirstOrDefault(fab =>
                                fab.Name.Equals(porSog.ServiceFabgr, StringComparison.OrdinalIgnoreCase));

                    updatedSogs.Add(new Sog
                    {
                        Alignment = porSog.Alignment,
                        Description = porSog.SOG,
                        HWProductDescription = porSog.HW_Produktbeschreibung,
                        Name = porSog.SOG_Code,
                        PlaId = pla.Id,
                        SFabId = sFab?.Id,

                        //TODO: Edit to appropriate column when Dirk returns
                        ServiceType = porSog.Alignment == "Software" ?
                                        ServiceType.Software : ServiceType.Hardware
                    });
                }

                var added = this.AddOrActivate(updatedSogs, modifiedDateTime);

                foreach (var addedEntity in added)
                {
                    _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADDED_OR_UPDATED_ENTITY,
                        nameof(Sog), addedEntity.Name);
                }

                _logger.Log(LogLevel.Info, PorImportLoggingMessage.ADD_STEP_END, added.Count);
            }

            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, ex, PorImportLoggingMessage.UNEXPECTED_ERROR);
                result = false;
            }

            return result;
        }

    }
}
