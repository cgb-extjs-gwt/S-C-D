using Gdc.Scd.Core.Interfaces;
using System;
using System.IO;

namespace Gdc.Scd.Export.Archive
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

            var blocks = repo.GetCostBlocks();
            for (var i = 0; i < blocks.Length; i++)
            {
                ProcessBlock(blocks[i]);
            }

            logger.Info(ArchiveConstants.END_PROCESS);
        }

        private void ProcessBlock(CostBlockDto b)
        {
            logger.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.TableName));

            Stream data = null;

            try
            {
                data = repo.GetData(b);
                repo.Save(b, null, data);
                logger.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.TableName, ". OK"));
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Process cost block " + b.TableName + " failed!");
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
