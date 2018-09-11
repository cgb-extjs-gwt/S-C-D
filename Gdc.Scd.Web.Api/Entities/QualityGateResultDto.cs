using System.Collections.Generic;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Entities
{
    public class QualityGateResultDto
    {
        public IEnumerable<IDictionary<string, object>> Errors { get; set; }

        public bool HasErrors { get; set; }

        public CostEditorContext Context { get; set; }

        public IEnumerable<EditItem> EditItems { get; set; }
    }
}
