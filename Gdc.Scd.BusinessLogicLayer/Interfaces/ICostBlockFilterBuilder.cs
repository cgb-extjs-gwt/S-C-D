using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICostBlockFilterBuilder
    {
        IDictionary<string, long[]> BuildRegionFilter(CostEditorContext context, IEnumerable<Country> userCountries = null);

        IDictionary<string, long[]> BuildFilter(CostEditorContext context, IEnumerable<Country> userCountries = null);
    }
}
