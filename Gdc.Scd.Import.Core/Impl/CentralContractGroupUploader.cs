using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Core.DataAccess;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Import.Core.Impl
{
    public class CentralContractGroupUploader : IUploader<CentralContractGroupDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<CentralContractGroup> _repositoryCentralContractGroup;
        private readonly IRepository<Wg> _repositoryWg;
        private readonly ILogger<LogLevel> _logger;

        public CentralContractGroupUploader(IRepositorySet repositorySet, ILogger<LogLevel> logger,
            ImportRepository<Wg> wgRepository)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryWg = wgRepository;
            this._repositoryCentralContractGroup = this._repositorySet.GetRepository<CentralContractGroup>();
            this._logger = logger;
        }

        public IEnumerable<UpdateQueryOption> Upload(IEnumerable<CentralContractGroupDto> items, DateTime modifiedDateTime)
        {
            UploadCentralContractGroup(items, modifiedDateTime);
            return UpdateWgs(items, modifiedDateTime);
        }

        private void UploadCentralContractGroup(IEnumerable<CentralContractGroupDto> items, DateTime modifiedDateTime)
        {
            var centralContractGroups = _repositoryCentralContractGroup.GetAll().ToList();

            var newCentralContractGroups = new Dictionary<string, CentralContractGroup>();

            foreach (var item in items)
            {
                var centralContractGroup = centralContractGroups.FirstOrDefault(ccg => ccg.Code.Equals(item.ZZWTY_CONTR_GR,
                    StringComparison.OrdinalIgnoreCase));

                //Central Contract Group does not exist in database -> add it
                if (centralContractGroup == null)
                {
                    _logger.Log(LogLevel.Debug, ImportConstants.ADD_NEW_CCG, item.ZZWTY_CONTR_GR);

                    CollectionHelper.AddEntry<CentralContractGroup>(newCentralContractGroups, new CentralContractGroup
                    {
                        Name = item.ZZWTY_CONTR_GR_D,
                        Code = item.ZZWTY_CONTR_GR
                    }, _logger);
                }

                else
                {
                    if (!centralContractGroup.Name.Equals(item.ZZWTY_CONTR_GR_D, 
                        StringComparison.OrdinalIgnoreCase))
                    {
                        centralContractGroup.Name = item.ZZWTY_CONTR_GR_D;
                        CollectionHelper.AddEntry<CentralContractGroup>(newCentralContractGroups, centralContractGroup, _logger);
                    }
                }
            }

            if (newCentralContractGroups.Any())
            {
                _repositoryCentralContractGroup.Save(newCentralContractGroups.Values);
                _repositorySet.Sync();
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_CCG_END, newCentralContractGroups.Count);
        }

        private IEnumerable<UpdateQueryOption> UpdateWgs(IEnumerable<CentralContractGroupDto> items, DateTime modifiedDateTime)
        {
            _logger.Log(LogLevel.Info, ImportConstants.UPDATING_WGS);
            var updateOption = new List<UpdateQueryOption>();
            var wgs = _repositoryWg.GetAll().ToList();
            var centralContractGroups = _repositoryCentralContractGroup.GetAll().ToList();

            var uploadedCcg = items.Select(i => i.ZZWTY_CONTR_GR).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var updatedWgs = new Dictionary<string, Wg>();

            foreach (var item in items)
            {
                var dbCcg = centralContractGroups.FirstOrDefault(ccg =>
                            ccg.Code.Equals(item.ZZWTY_CONTR_GR, StringComparison.OrdinalIgnoreCase));

                if (dbCcg != null)
                {
                    var wg = wgs.FirstOrDefault(w =>
                            w.Name.Equals(item.ZZWTY_WTY_GRP, StringComparison.OrdinalIgnoreCase));

                    if (wg == null)
                    {
                        _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_WARRANTY, item.ZZWTY_WTY_GRP);
                        continue;
                    }

                    if (wg.CentralContractGroupId != dbCcg.Id)
                    {
                        updateOption.Add(
                                new UpdateQueryOption(
                                    new Dictionary<string, long>
                                    {
                                        [MetaConstants.WgInputLevelName] = wg.Id,
                                        [MetaConstants.CentralContractGroupInputLevel] = wg.CentralContractGroupId.Value
                                    },
                                    new Dictionary<string, long>
                                    {
                                        [MetaConstants.WgInputLevelName] = wg.Id,
                                        [MetaConstants.CentralContractGroupInputLevel] = dbCcg.Id
                                    }));
                        wg.CentralContractGroupId = dbCcg.Id;

                        CollectionHelper.AddEntry<Wg>(updatedWgs, wg, _logger);
                    }
                }
            }

            if (updatedWgs.Any())
            {
                _repositoryWg.Save(updatedWgs.Values);
                _repositorySet.Sync();
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPDATING_WGS);
            return updateOption;
        }
    }
}
