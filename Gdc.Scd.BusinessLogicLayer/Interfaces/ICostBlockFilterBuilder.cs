using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockFilterBuilder
    {
        IDictionary<string, IEnumerable<object>> BuildRegionFilter(CostEditorContext context);

        IDictionary<string, IEnumerable<object>> BuildFilter(CostEditorContext context);
    }
}
