using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockHistoryService
    {
        IQueryable<CostBlockHistory> GetHistories();

        Task Save(CostEditorContext context, IEnumerable<EditItem> editItems);
    }
}
