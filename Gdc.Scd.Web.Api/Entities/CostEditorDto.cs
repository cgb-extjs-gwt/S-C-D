using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Meta.Entities;

namespace Gdc.Scd.Web.Api.Entities
{
    public class CostEditorDto
    {
        public DomainMeta Meta { get; set; }

        public IEnumerable<string> Countries { get; set; }
    }
}
