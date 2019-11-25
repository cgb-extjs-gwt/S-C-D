using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Export.CdCs.Helpers;
using Gdc.Scd.Export.CdCs.Procedures;
using System.IO;

namespace Gdc.Scd.Export.CdCs
{
    public class CdCsService
    {
        protected SharePointClient spClient;

        protected ILogger Logger;

        protected GetServiceCostsBySla getServiceCostsBySla;
        protected GetProActiveCosts getProActiveCosts;
        protected GetHddRetentionCosts getHddRetentionCosts;
        protected ConfigHandler configHandler;

        private SlaCollection slaList;

        public CdCsService(
                IRepositorySet repo,
                SharePointClient spClient,
                ILogger log
            )
        {
            this.spClient = spClient;
            this.Logger = log;

            configHandler = new ConfigHandler(repo);
            getServiceCostsBySla = new GetServiceCostsBySla(repo);
            getProActiveCosts = new GetProActiveCosts(new CommonService(repo));
            getHddRetentionCosts = new GetHddRetentionCosts(repo);
        }

        protected CdCsService() { }

        public virtual void Run()
        {
            slaList = null;

            Logger.Info(CdCsMessages.START_PROCESS);

            Logger.Info(CdCsMessages.READ_CONFIGURATION);
            var configList = configHandler.ReadAllConfiguration();

            Logger.Info(CdCsMessages.READ_TEMPLATE);
            using (var excel = spClient.Load(Config.SpFile))
            {
                Logger.Info(CdCsMessages.WRITE_COUNTRY_COSTS);
                foreach (var config in configList)
                {
                    PrecessExcel(excel, config);
                }
            }

            Logger.Info(CdCsMessages.END_PROCESS);
        }

        protected virtual void PrecessExcel(Stream excel, CdCsConfiguration config)
        {
            using (var writer = new ExcelWriter(excel))
            {
                if (slaList == null)
                {
                    slaList = writer.ReadSla();
                }

                WriteExcel(writer, slaList, config);

                //UPLOAD
                Logger.Info(CdCsMessages.UPLOAD_FILE);
                spClient.Send(writer.GetData(), config);
            }
        }

        protected void WriteExcel(ExcelWriter writer, SlaCollection slaList, CdCsConfiguration config)
        {
            Logger.Info(CdCsMessages.READ_COUNTRY_COSTS, config.GetCountry());

            WriteServiceCost(writer, slaList, config);
            WriteProactive(writer, config);
            WriteHdd(writer, config);
            writer.WriteToolConfig(config.GetCountry(), config.GetPriceListPath());
        }

        protected void WriteServiceCost(ExcelWriter writer, SlaCollection slaList, CdCsConfiguration config)
        {
            Logger.Info(CdCsMessages.READ_SERVICE);
            var data = getServiceCostsBySla.Execute(config.CountryId, slaList);

            Logger.Info(CdCsMessages.WRITE_SERVICE);
            writer.WriteTcTp(data);
        }

        protected void WriteProactive(ExcelWriter writer, CdCsConfiguration config)
        {
            Logger.Info(CdCsMessages.READ_PROACTIVE);
            var data = getProActiveCosts.Execute(config.GetCountry());

            Logger.Info(CdCsMessages.WRITE_PROACTIVE);
            writer.WriteProactive(data, config.GetCurrency());
        }

        protected void WriteHdd(ExcelWriter writer, CdCsConfiguration config)
        {
            Logger.Info(CdCsMessages.READ_HDD_RETENTION);
            var data = getHddRetentionCosts.Execute(config.GetCountry());

            Logger.Info(CdCsMessages.WRITE_HDD_RETENTION);
            writer.WriteHdd(data, config.GetCurrency());
        }
    }
}
