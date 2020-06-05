using System;
using System.IO;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.ArchiveJob.Dto;

namespace Gdc.Scd.Export.ArchiveJob
{
    public class ArchiveService
    {
        protected IArchiveRepository repo;

        protected ILogger logger;

        public ArchiveService(IArchiveRepository repo, ILogger logger)
        {
            this.repo = repo;
            this.logger = logger;
        }

        public virtual void Run()
        {
            logger.Info(ArchiveConstants.START_PROCESS);

            //load cost blocks...
            var blocks = repo.GetCostBlocks();
            for (var i = 0; i < blocks.Length; i++)
            {
                Process(blocks[i]);
            }

            //load hardware cost calculations...
            var countries = repo.GetCountries();
            for (var i = 0; i < countries.Length; i++)
            {
                Process(countries[i]);
            }

            logger.Info(ArchiveConstants.END_PROCESS);
        }

        protected virtual void Process(ArchiveDto b)
        {
            logger.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.ArchiveName));

            Stream data = null;

            try
            {
                data = repo.GetData(b);
                repo.Save(b, data);
                logger.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.ArchiveName, ". OK"));
            }
            catch (Exception e)
            {
                logger.Fatal(e, string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.ArchiveName, " failed!"));
                throw;
            }
            finally
            {
                if (data != null)
                {
                    data.Dispose();
                }
            }
        }

        protected virtual void Process(CountryDto cnt)
        {
            logger.Info(string.Concat(ArchiveConstants.PROCESS_COUNTRY, " ", cnt.Name));

            var archives = repo.GetCountryArchives();
            for(var i = 0; i < archives.Length; i++)
            {
                Process(cnt, archives[i]);
            }
        }

        protected virtual void Process(CountryDto cnt, ArchiveDto archive)
        {
            var log = string.Concat(ArchiveConstants.PROCESS_COUNTRY, " ", cnt.Name, " ", archive.ArchiveName);

            Stream data = null;

            try
            {
                data = repo.GetData(cnt, archive);
                repo.Save(cnt, archive, data);
                logger.Info(string.Concat(log, ". OK"));
            }
            catch (Exception e)
            {
                logger.Fatal(e, string.Concat(log, " failed!"));
                throw;
            }
            finally
            {
                if (data != null)
                {
                    data.Dispose();
                }
            }
        }
    }
}
