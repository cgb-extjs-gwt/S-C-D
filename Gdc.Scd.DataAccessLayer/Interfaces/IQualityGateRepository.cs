using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface IQualityGateRepository
    {
        Task<IEnumerable<QualityGateError>> Check(HistoryContext historyContext, IEnumerable<EditItem> editItems, IDictionary<string, IEnumerable<object>> costBlockFilter);
    }
}
