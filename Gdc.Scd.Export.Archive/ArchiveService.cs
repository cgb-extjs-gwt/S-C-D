using System;
using System.IO;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.ArchiveJob.Dto;

namespace Gdc.Scd.Export.ArchiveJob
{
    public class ArchiveService
    {
        private IArchiveRepository repo;

        private ILogger logger;

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

        private void Process(CostBlockDto b)
        {
            logger.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.TableName));

            Stream data = null;

            try
            {
                data = repo.GetData(b);
                repo.Save(b, data);
                logger.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.TableName, ". OK"));
            }
            catch (Exception e)
            {
                logger.Fatal(e, string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.TableName, " failed!"));
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

        private void Process(CountryDto cnt)
        {
            logger.Info(string.Concat(ArchiveConstants.PROCESS_COUNTRY_HW, " ", cnt.Name));

            Stream data = null;

            try
            {
                data = repo.GetData(cnt);
                repo.Save(cnt, data);
                logger.Info(string.Concat(ArchiveConstants.PROCESS_COUNTRY_HW, " ", cnt.Name, ". OK"));
            }
            catch (Exception e)
            {
                logger.Fatal(e, string.Concat(ArchiveConstants.PROCESS_COUNTRY_HW, " ", cnt.Name, " failed!"));
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
