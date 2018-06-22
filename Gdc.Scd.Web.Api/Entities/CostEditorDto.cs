using System.Collections.Generic;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Web.Api.Entities
{
    public class CostEditorDto
    {
        public DomainMeta Meta { get; set; }

        public IEnumerable<string> Countries { get; set; }
    }
}
