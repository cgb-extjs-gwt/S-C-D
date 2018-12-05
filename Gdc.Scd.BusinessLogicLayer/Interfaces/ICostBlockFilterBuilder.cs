using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockFilterBuilder
    {
        IDictionary<string, long[]> BuildRegionFilter(HistoryContext context, IEnumerable<Country> userCountries = null);

        IDictionary<string, long[]> BuildCoordinateFilter(CostEditorContext context);

        IDictionary<string, long[]> BuildFilter(CostEditorContext context, IEnumerable<Country> userCountries = null);
    }
}
