using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Entities.TableView;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ITableViewService
    {
        Task<IEnumerable<Record>> GetRecords();

        Task<QualityGateResultSet> UpdateRecords(IEnumerable<Record> records, ApprovalOption approvalOption);

        Task<TableViewInfo> GetTableViewInfo();

        Task<DataInfo<HistoryItemDto>> GetHistory(CostElementIdentifier costElementId, IDictionary<string, long> coordinates, QueryInfo queryInfo = null);

        Task<Stream> ExportToExcel();
    }
}
