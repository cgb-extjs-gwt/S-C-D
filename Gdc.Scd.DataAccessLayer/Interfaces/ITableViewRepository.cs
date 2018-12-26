using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ITableViewRepository
    {
        Task<IEnumerable<Record>> GetRecords(CostElementInfo[] costBlockInfos);

        Task UpdateRecords(IEnumerable<EditInfo> editInfos);

        Task<IDictionary<string, IEnumerable<NamedId>>> GetReferences(CostElementInfo[] costBlockInfos);

        Task<IDictionary<string, IEnumerable<NamedId>>> GetDependencyItems(CostElementInfo[] costBlockInfos);

        RecordInfo GetRecordInfo(CostElementInfo[] costBlockInfos);

        IEnumerable<EditInfo> BuildEditInfos(CostElementInfo[] costBlockInfos, IEnumerable<Record> records);
    }
}
