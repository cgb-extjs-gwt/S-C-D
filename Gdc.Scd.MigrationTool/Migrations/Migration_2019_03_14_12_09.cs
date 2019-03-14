using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Interfaces;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_14_12_09 : IMigrationAction
    {
        private readonly IRepositorySet repositorySet;

        public int Number => 39;

        public string Description => "Rename column 'Alias Region' to 'Region'";

        public Migration_2019_03_14_12_09(IRepositorySet repositorySet)
        {
            this.repositorySet = repositorySet;
        }

        public void Execute()
        {
            var reportRepository = this.repositorySet.GetRepository<Report>();
            var reportColumnRepository = this.repositorySet.GetRepository<ReportColumn>();

            var report = reportRepository.GetAll().Where(rep => rep.Name.ToUpper() == "LOGISTIC-COST-COUNTRY").FirstOrDefault();
            var column = reportColumnRepository.GetAll().Where(col => col.Report == report && col.Name == "Region").FirstOrDefault();
            if (column != null)
            {
                column.Text = "Report";
            }
            reportColumnRepository.Save(column);

            report = reportRepository.GetAll().Where(rep=>rep.Name.ToUpper()== "LOGISTIC-COST-INPUT-COUNTRY").FirstOrDefault();
            column = reportColumnRepository.GetAll().Where(col => col.Report == report && col.Name=="Region").FirstOrDefault();
            if (column != null)
            {
                column.Text = "Report";
            }
            reportColumnRepository.Save(column);


            report = reportRepository.GetAll().Where(rep => rep.Name.ToUpper() == "LOGISTIC-COST-CALC-COUNTRY").FirstOrDefault();
            column = reportColumnRepository.GetAll().Where(col => col.Report == report && col.Name == "Region").FirstOrDefault();
            if (column != null)
            {
                column.Text = "Report";
            }
            reportColumnRepository.Save(column);

            this.repositorySet.Sync();
        }
    }
}
