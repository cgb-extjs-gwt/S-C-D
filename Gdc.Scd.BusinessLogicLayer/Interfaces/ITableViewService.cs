using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities.TableView;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ITableViewService
    {
        Task<IEnumerable<Record>> GetRecords();

        Task UpdateRecords(IEnumerable<Record> records, bool isApproving);

        Task<TableViewInfo> GetTableViewInfo();
    }
}
