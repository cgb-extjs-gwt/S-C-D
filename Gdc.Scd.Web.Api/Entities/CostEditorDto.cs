using System.Collections.Generic;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Meta.Entities;

namespace Gdc.Scd.Web.Api.Entities
{
    public class CostEditorDto
    {
        public DomainMeta Meta { get; set; }

        public IEnumerable<Country> Countries { get; set; }
    }
}
