using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostEditorRepository
    {
        Task<IEnumerable<EditItem>> GetEditItems(EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null);

        Task<IEnumerable<EditItem>> GetEditItemsByLevel(string levelColumn, EditItemInfo editItemInfo, IDictionary<string, IEnumerable<object>> filter = null);
    }
}
