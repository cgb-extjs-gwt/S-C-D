using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Impl
{
    public class SfabUploader : IUploader<SFabDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<SFab> _repositorySfab;
        private readonly IRepository<Wg> _repositoryWg;
        private readonly IRepository<Sog> _repositorySog;
        private readonly ILogger<LogLevel> _logger;

        public SfabUploader(IRepositorySet repositorySet, ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryWg = this._repositorySet.GetRepository<Wg>();
            this._repositorySfab = this._repositorySet.GetRepository<SFab>();
            this._repositorySog = this._repositorySet.GetRepository<Sog>();
            this._logger = logger;
        }
        public void Upload(IEnumerable<SFabDto> items, DateTime modifiedDateTime, 
            List<UpdateQueryOption> updateOption = null)
        {
            UploadSfabs(items, modifiedDateTime);
            DeactivateSfabs(items, modifiedDateTime);
            UpdateWgsAndSogs(items, modifiedDateTime, updateOption);
        }

        public void UploadSfabs(IEnumerable<SFabDto> items, DateTime modifiedDateTime)
        {
            var sfabs = _repositorySfab.GetAll().ToList();
            var wgs = _repositoryWg.GetAll().Where(w => w.WgType == Scd.Core.Enums.WgType.Por).ToList();

            var newSfabs = new Dictionary<string, SFab>();

            foreach (var item in items)
            {
                var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.WarrantyGroup,
                    StringComparison.OrdinalIgnoreCase));

                if (wg == null)
                {
                    continue;
                }

                var sfab = sfabs.FirstOrDefault(sf => sf.Name.Equals(item.Sfab,
                    StringComparison.OrdinalIgnoreCase));

                //SFab does not exist in database -> add it
                if (sfab == null)
                {
                    _logger.Log(LogLevel.Debug, ImportConstants.ADD_NEW_SFAB, item.Sfab);
                    AddEntry<SFab>(newSfabs, new SFab
                    {
                        Name = item.Sfab,
                        PlaId = wg.PlaId
                    });
                }

                else
                {
                    if (sfab.PlaId != wg.PlaId)
                    {
                        sfab.PlaId = wg.PlaId;
                        AddEntry<SFab>(newSfabs, sfab);
                    }
                }  
            }

            if (newSfabs.Any())
            {
                _repositorySfab.Save(newSfabs.Values);
                _repositorySet.Sync();
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_SFAB_END, newSfabs.Count);

        }

        public void DeactivateSfabs(IEnumerable<SFabDto> items, DateTime modifiedDateTime)
        {
            _logger.Log(LogLevel.Info, ImportConstants.DEACTIVATING_SFAB_START);
            var sfabs = items.Select(i => i.Sfab).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var notActiveSfabs = _repositorySfab.GetAll()
                .Where(sf => !sfabs.Contains(sf.Name, StringComparer.OrdinalIgnoreCase) && !sf.DeactivatedDateTime.HasValue)
                .ToList();

            foreach (var notActiveSfab in notActiveSfabs)
            {
                _logger.Log(LogLevel.Debug, ImportConstants.DEACTIVATING_SFAB, notActiveSfab.Name);
                notActiveSfab.DeactivatedDateTime = modifiedDateTime;
            }

            if (notActiveSfabs.Any())
            {
                _repositorySfab.Save(notActiveSfabs);
                _repositorySet.Sync();
            }
           
            _logger.Log(LogLevel.Info, ImportConstants.DEACTIVATING_SFAB_END, notActiveSfabs.Count);
        }

        public void UpdateWgsAndSogs(IEnumerable<SFabDto> items, DateTime modifiedDateTime,
            List<UpdateQueryOption> updateOption)
        {
            _logger.Log(LogLevel.Info, ImportConstants.UPDATING_WGS_AND_SOGS_START);
            var sfabs = _repositorySfab.GetAll().Where(sf => !sf.DeactivatedDateTime.HasValue).ToList();
            var wgs = _repositoryWg.GetAll().Where(w => w.WgType == Scd.Core.Enums.WgType.Por).ToList();
            var sogs = _repositorySog.GetAll().ToList();
            var uploadedSfabs = items.Select(i => i.Sfab).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var updatedWgs = new Dictionary<string, Wg>();
            var updatedSogs = new Dictionary<string, Sog>();

            foreach (var item in items)
            {
                var dbSfab = sfabs.FirstOrDefault(sf => 
                            sf.Name.Equals(item.Sfab, StringComparison.OrdinalIgnoreCase));

                if (dbSfab != null)
                {
                    var wg = wgs.FirstOrDefault(w =>
                            w.Name.Equals(item.WarrantyGroup, StringComparison.OrdinalIgnoreCase));
                    if (wg == null)
                    {
                        _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_WARRANTY, item.WarrantyGroup);
                        continue;
                    }

                    if (wg.SFabId != dbSfab.Id)
                    {
                        if (wg.SFabId.HasValue)
                        {
                            updateOption.Add(
                                new UpdateQueryOption(
                                    new Dictionary<string, long>
                                    {
                                        [MetaConstants.WgInputLevelName] = wg.Id,
                                        [MetaConstants.SfabInputLevel] = wg.SFabId.Value
                                    },
                                    new Dictionary<string, long>
                                    {
                                        [MetaConstants.WgInputLevelName] = wg.Id,
                                        [MetaConstants.SfabInputLevel] = dbSfab.Id
                                    }));
                        }

                        wg.SFabId = dbSfab.Id;
                        AddEntry<Wg>(updatedWgs, wg);
                    }

                    if (wg.SogId != null)
                    {
                        var sog = sogs.FirstOrDefault(s => s.Id == wg.SogId);
                        if (sog != null && sog.SFabId != dbSfab.Id)
                        {
                            if (sog.SFabId.HasValue)
                            {
                                updateOption.Add(
                                new UpdateQueryOption(
                                    new Dictionary<string, long>
                                    {
                                        [MetaConstants.SogInputLevel] = sog.Id,
                                        [MetaConstants.SfabInputLevel] = sog.SFabId.Value
                                    },
                                    new Dictionary<string, long>
                                    {
                                        [MetaConstants.SogInputLevel] = sog.Id,
                                        [MetaConstants.SfabInputLevel] = dbSfab.Id
                                    }));
                            }

                            sog.SFabId = dbSfab.Id;
                            AddEntry<Sog>(updatedSogs, sog);
                        }
                    }
                }
            }

            if (updatedWgs.Any())
            {
                _repositoryWg.Save(updatedWgs.Values);
                _repositorySet.Sync();
            }

            if (updatedSogs.Any())
            {
                _repositorySog.Save(updatedSogs.Values);
                _repositorySet.Sync();
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPDATING_WGS_AND_SOGS_END);
        }

        private void AddEntry<T>(Dictionary<string, T> collection, T item) where T: NamedId
        {
            if (!collection.ContainsKey(item.Name))
            {
                _logger.Log(LogLevel.Debug, ImportConstants.UPDATING_ENTITY, typeof(T).Name, item.Name);
                collection.Add(item.Name, item);
            }
        }
    }
}
