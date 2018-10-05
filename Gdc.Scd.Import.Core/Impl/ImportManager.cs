using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Impl
{
    public class ImportManager<T> : IImportManager
    {
        private readonly ILogger<LogLevel> _logger;
        private readonly IDownloader _downloader;
        private readonly IParser<T> _parser;
        private readonly IUploader<T> _uploader;

        public ImportManager(ILogger<LogLevel> logger, 
            IDownloader downloader, IParser<T> parser, 
            IUploader<T> uploader)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (downloader == null)
                throw new ArgumentNullException(nameof(downloader));

            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            if (uploader == null)
                throw new ArgumentNullException(nameof(uploader));

            _logger = logger;
            _downloader = downloader;
            _parser = parser;
            _uploader = uploader;
        }

        public void ImportData(ImportConfiguration configuration)
        {
            var downloadDto = new DownloadInfoDto {
                File = configuration.FileName,
                Path = configuration.FilePath,
                ProcessedFilePath = configuration.ProcessedFilesPath };

            _logger.Log(LogLevel.Info, ImportConstants.CHECK_LAST_MODIFIED_DATE);
            var modifiedDateTime = _downloader.GetModifiedDateTime(downloadDto);
            _logger.Log(LogLevel.Info, ImportConstants.CHECK_LAST_MODIFIED_DATE_END, modifiedDateTime.ToString());

            _logger.Log(LogLevel.Info, ImportConstants.CHECK_CONFIGURATION, 
                configuration.FileName, configuration.ProcessedDateTime?.ToLongDateString(), configuration.Occurancy);

            if (!configuration.ProcessedDateTime.HasValue || 
                ShouldUpload(configuration.Occurancy, modifiedDateTime, configuration.ProcessedDateTime.Value))
            {
                _logger.Log(LogLevel.Info, ImportConstants.DOWNLOAD_FILE_START, configuration.FileName);
                var downloadedInfo = _downloader.DownloadData(downloadDto);
                _logger.Log(LogLevel.Info, ImportConstants.DOWNLOAD_FILE_END);
                IEnumerable<T> entities = null;

                using (downloadedInfo)
                {
                    var parsedModel = new ParseInfoDto
                    {
                        Content = downloadedInfo,
                        Delimeter = configuration.Delimeter,
                        HasHeader = configuration.HasHeader
                    };

                    _logger.Log(LogLevel.Info, ImportConstants.PARSE_START);
                    entities = _parser.Parse(parsedModel);
                    _logger.Log(LogLevel.Info, ImportConstants.PARSE_END, entities?.Count());
                }

                if (entities != null && entities.Any())
                {
                    _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_START);
                    int result = _uploader.Upload(entities, DateTime.Now);
                    _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_END, result);
                    _logger.Log(LogLevel.Info, ImportConstants.DEACTIVATE_START, nameof(TaxAndDutiesEntity));
                    result = _uploader.Deactivate(DateTime.Now);
                    _logger.Log(LogLevel.Info, ImportConstants.DEACTIVATE_END, result);
                    _logger.Log(LogLevel.Info, ImportConstants.MOVE_FILE_START, configuration.ProcessedFilesPath);
                    _downloader.MoveFile(downloadDto);
                    _logger.Log(LogLevel.Info, ImportConstants.MOVE_FILE_END);
                }
                else
                {
                    _logger.Log(LogLevel.Error, ImportConstants.PARSE_INVALID_FILE);
                    throw new Exception($"File is invalid. Please check file {Path.Combine(configuration.FilePath, configuration.FileName)}");
                }
                
            }
            else
            {
                _logger.Log(LogLevel.Info, ImportConstants.SKIP_UPLOADING);
            }
        }

        private bool ShouldUpload(Occurancy occurancy, DateTime modifiedDateTime, 
            DateTime lastRunDateTime)
        {
            switch (occurancy)
            {
                case Occurancy.PerMonth:
                    return modifiedDateTime >= lastRunDateTime.AddMonths(1);
                case Occurancy.PerWeek:
                    return modifiedDateTime >= lastRunDateTime.AddDays(7);
                case Occurancy.PerDay:
                    return true;
                default:
                    throw new Exception($"Unknown Occurancy: {occurancy}");
            }
        }
    }
}
