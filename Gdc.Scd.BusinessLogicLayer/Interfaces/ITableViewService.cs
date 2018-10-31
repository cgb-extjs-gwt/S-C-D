using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities.TableView;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ITableViewService
    {
        Task<IEnumerable<Record>> GetRecords();

        void UpdateRecords(IEnumerable<Record> records, ApprovalOption approvalOption);

        Task<TableViewInfo> GetTableViewInfo();
    }
}
