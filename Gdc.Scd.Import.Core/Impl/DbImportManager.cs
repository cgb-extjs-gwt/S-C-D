using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
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
    public class DbImportManager<T> : IImportManager
    {
        private readonly ILogger<LogLevel> _logger;
        private readonly IDataImporter<T> _downloader;
        private readonly IUploader<T> _uploader;

        public DbImportManager(ILogger<LogLevel> logger,
            IDataImporter<T> downloader,
            IUploader<T> uploader)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (downloader == null)
                throw new ArgumentNullException(nameof(downloader));

            if (uploader == null)
                throw new ArgumentNullException(nameof(uploader));

            _logger = logger;
            _downloader = downloader;
            _uploader = uploader;
        }

        public ImportResultDto ImportData(ImportConfiguration configuration = null)
        {
            var importResult = new ImportResultDto();
            _logger.Log(LogLevel.Info, ImportConstants.IMPORT_DATA_STARTED, nameof(T));
            var entities = _downloader.ImportData().ToList();
            _logger.Log(LogLevel.Info, ImportConstants.IMPORT_DATA_END, entities.Count);
            if (entities != null && entities.Any())
            {
                _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_START);
                importResult.UpdateOptions = _uploader.Upload(entities, DateTime.Now);
            }

            return importResult;
        }
    }
}
