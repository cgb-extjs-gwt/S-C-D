using System.Collections.Generic;

namespace Gdc.Scd.Core.Entities.TableView
{
    public class ReferenceSet
    {
        public IDictionary<string, IEnumerable<NamedId>> References { get; set; }
    }
}
