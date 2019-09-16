using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gdc.Scd.Import.Core.Impl
{
    public class FileImportManager<T> : IImportManager
    {
        private readonly ILogger<LogLevel> _logger;
        private readonly IDownloader _downloader;
        private readonly IParser<T> _parser;
        private readonly IUploader<T> _uploader;

        public FileImportManager(ILogger<LogLevel> logger, 
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

        public ImportResultDto ImportData(ImportConfiguration configuration)
        {
            var importResult = new ImportResultDto();

            var downloadDto = new DownloadInfoDto {
                File = configuration.FileName,
                Path = configuration.FilePath,
                ProcessedFilePath = configuration.ProcessedFilesPath };

            _logger.Log(LogLevel.Info, ImportConstants.CHECK_LAST_MODIFIED_DATE);
            var modifiedDateTime = _downloader.GetModifiedDateTime(downloadDto);
            if (!modifiedDateTime.HasValue)
            {
                _logger.Log(LogLevel.Info, ImportConstants.FILE_WASNT_DELIVERED);
                importResult.Skipped = true;
                return importResult;
            }

            _logger.Log(LogLevel.Info, ImportConstants.CHECK_LAST_MODIFIED_DATE_END, modifiedDateTime.ToString());

            importResult.ModifiedDateTime = modifiedDateTime.Value;

            _logger.Log(LogLevel.Info, ImportConstants.CHECK_CONFIGURATION, 
                configuration.FileName, configuration.ProcessedDateTime?.ToLongDateString());

            if (!configuration.ProcessedDateTime.HasValue || 
                modifiedDateTime.Value > configuration.ProcessedDateTime.Value)
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
                        HasHeader = configuration.HasHeader,
                        Culture = configuration.Culture
                    };

                    _logger.Log(LogLevel.Info, ImportConstants.PARSE_START);
                    entities = _parser.Parse(parsedModel);
                    _logger.Log(LogLevel.Info, ImportConstants.PARSE_END, entities?.Count());
                }

                if (entities != null && entities.Any())
                {
                    _logger.Log(LogLevel.Info, ImportConstants.UPLOAD_START);
                    importResult.UpdateOptions = _uploader.Upload(entities, DateTime.Now);
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
                importResult.Skipped = true;
                _logger.Log(LogLevel.Info, ImportConstants.SKIP_UPLOADING);
            }

            return importResult;
        }
    }
}
