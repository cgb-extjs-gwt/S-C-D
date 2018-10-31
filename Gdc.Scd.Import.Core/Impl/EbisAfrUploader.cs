using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
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
    public class EbisAfrUploader : IUploader<AfrDto>
    {
        private readonly IRepositorySet _repositorySet;
        private readonly IRepository<Year> _repositoryYear;
        private readonly IRepository<Wg> _repositoryWg;
        private readonly IRepository<Afr> _repositoryAfr;
        private readonly ILogger<LogLevel> _logger;

        public EbisAfrUploader(IRepositorySet repositorySet, ILogger<LogLevel> logger)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this._repositorySet = repositorySet;
            this._repositoryWg = this._repositorySet.GetRepository<Wg>();
            this._repositoryYear = this._repositorySet.GetRepository<Year>();
            this._repositoryAfr = this._repositorySet.GetRepository<Afr>();
            this._logger = logger;
        }

        public void Upload(IEnumerable<AfrDto> items, DateTime modifiedDateTime)
        {
            var wgs = _repositoryWg.GetAll().ToList();
            var years = _repositoryYear.GetAll().ToList();
            var afrs = _repositoryAfr.GetAll().ToList();

            var batchList = new List<Afr>();

            foreach (var item in items)
            {
                if (String.IsNullOrEmpty(item.Wg) || item.Wg.Equals("-") || !item.Year.HasValue)
                    continue;
                var wg = wgs.FirstOrDefault(w => w.Name.Equals(item.Wg, StringComparison.OrdinalIgnoreCase));
                if (wg == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_WARRANTY, item.Wg);
                    continue;
                }
                //Fill only non prolongation values. Prolongation must be filled by PSMs
                var year = years.FirstOrDefault(y => y.Value == item.Year.Value && !y.IsProlongation);
                if (year == null)
                {
                    _logger.Log(LogLevel.Warn, ImportConstants.UNKNOWN_YEAR, item.Year.Value);
                    continue;
                }

                var afrDb = afrs.FirstOrDefault(af => af.WgId == wg.Id && af.YearId == year.Id);
                if (afrDb == null)
                {
                    afrDb = new Afr();
                    afrDb.YearId = year.Id;
                    afrDb.WgId = wg.Id;
                }
                afrDb.AFR = item.Afr;
                batchList.Add(afrDb);
            }

            if (batchList.Any())
            {
                _repositoryAfr.Save(batchList);
            }

            _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_END, batchList.Count);
        }
    }
}
