using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities
{
    public class ValuesInfo
    {
        //key - the name of the dependency or input level, eg. Country, Wg, ServiceLocation
        //value - id of the dependency
        public IDictionary<string, long[]> CoordinateFilter { get; set; }

        //key - cost block element name
        //value - value to update, e.g value for TravelCost element in FieldServiceCost
        public IDictionary<string, object> Values { get; set; }
    }
}
