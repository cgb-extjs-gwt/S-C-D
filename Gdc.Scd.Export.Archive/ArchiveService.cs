﻿using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Export.Archive.Impl;
using System;
using System.IO;
using System.Threading.Tasks;

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

            //RunParallel(blocks);
            RunSequential(blocks);

            logger.Info(ArchiveConstants.END_PROCESS);
        }

        public virtual void RunSequential(CostBlockDto[] blocks)
        {
            for (var i = 0; i < blocks.Length; i++)
            {
                ProcessBlock(logger, blocks[i]);
            }
        }

        private void RunParallel(CostBlockDto[] blocks)
        {
            var len = blocks.Length;
            var tasks = new Subtask[len];

            for (var i = 0; i < len; i++)
            {
                var block = blocks[i];
                var buflog = new BufferedLogger(logger);
                //
                tasks[i] = new Subtask(buflog, block, () => ProcessBlock(buflog, block));
                tasks[i].Start();
            }

            var whenall=  Task.WhenAll(tasks).ContinueWith(x =>
            {
                for (var i = 0; i < tasks.Length; i++)
                {
                    tasks[i].WriteLog();
                }
            });

            whenall.Wait();
        }

        private void ProcessBlock(ILogger log, CostBlockDto b)
        {
            log.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.TableName));

            Stream data = null;

            try
            {
                data = repo.GetData(b);
                repo.Save(b, null, data);
                log.Info(string.Concat(ArchiveConstants.PROCESS_BLOCK, " ", b.TableName, ". OK"));
            }
            catch (Exception e)
            {
                log.Fatal(e, "Process cost block " + b.TableName + " failed!");
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

        class Subtask : Task
        {
            private CostBlockDto costBlock;

            private BufferedLogger log;

            public Subtask(BufferedLogger log, CostBlockDto block, Action action) : base(action)
            {
                this.costBlock = block;
                this.log = log;
            }

            public void WriteLog()
            {
                this.log.Flush();
            }
        }


    }
}
