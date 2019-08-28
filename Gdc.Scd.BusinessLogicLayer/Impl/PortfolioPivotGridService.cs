using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities.Pivot;
using Gdc.Scd.Core.Entities.Portfolio;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class PortfolioPivotGridService : IPortfolioPivotGridService
    {
        private readonly DomainEnitiesMeta meta;

        private readonly IPivotGridRepository pivotGridRepository;

        private readonly IPortfolioPivotGridQueryBuilder portfolioPivotGridQueryBuilder;

        public PortfolioPivotGridService(
            DomainEnitiesMeta meta, 
            IPivotGridRepository pivotGridRepository, 
            IPortfolioPivotGridQueryBuilder portfolioPivotGridQueryBuilder)
        {
            this.meta = meta;
            this.pivotGridRepository = pivotGridRepository;
            this.portfolioPivotGridQueryBuilder = portfolioPivotGridQueryBuilder;
        }

        public async Task<PivotResult> GetData(PortfolioPivotRequest request)
        {
            var queryMeta = this.portfolioPivotGridQueryBuilder.Build(request);

            return await this.pivotGridRepository.GetData(request, queryMeta.Meta, queryMeta.Query);
        }

        public async Task<Stream> PivotExcelExport(PortfolioPivotRequest request)
        {
            Stream stream;

            var queryMeta = this.portfolioPivotGridQueryBuilder.Build(request);
            var dataTable = await this.pivotGridRepository.GetExportData(request, queryMeta.Meta, queryMeta.Query);

            using (var workbook = new XLWorkbook())
            {
                var pivotSheet = workbook.Worksheets.Add("PivotTable");
                var dataSheet = workbook.Worksheets.Add("Data");
                var dataRange = dataSheet.Cell(1, 1).InsertTable(dataTable, true);
                var pivotTable = pivotSheet.PivotTables.Add("PivotTable", pivotSheet.Cell(1, 1), dataRange.AsRange());

                foreach (var item in request.LeftAxis)
                {
                    pivotTable.RowLabels.Add(item.Header);
                }

                foreach (var item in request.TopAxis)
                {
                    pivotTable.ColumnLabels.Add(item.Header);
                }

                foreach (var item in request.Aggregate)
                {
                    pivotTable.Values.Add(item.Header);
                }

                stream = new MemoryStream();

                workbook.SaveAs(stream);

                stream.Position = 0;
            }

            return stream;
        }
    }
}
