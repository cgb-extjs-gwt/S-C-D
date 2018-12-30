using System.Collections.Generic;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ICostBlockFilterBuilder
    {
        IDictionary<string, long[]> BuildRegionFilter(HistoryContext context, IEnumerable<Country> userCountries = null);

        IDictionary<string, long[]> BuildCoordinateFilter(CostEditorContext context);

        IDictionary<string, IEnumerable<object>> BuildCoordinateItemsFilter(BaseEntityMeta coordinateMeta);

        IDictionary<string, long[]> BuildFilter(CostEditorContext context, IEnumerable<Country> userCountries = null);
    }
}
