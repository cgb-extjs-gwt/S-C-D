using System.Collections.Generic;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostEditorRepository
    {
        Task<IEnumerable<EditItem>> GetEditItems(EditItemInfo editItemInfo, IDictionary<string, long[]> filter);

        Task<IEnumerable<EditItem>> GetEditItems(EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null);

        Task<int> UpdateValues(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null);

        Task<int> UpdateValues(IEnumerable<EditItem> editItems, EditItemInfo editItemInfo, IDictionary<string, long[]> filter);
    }
}
