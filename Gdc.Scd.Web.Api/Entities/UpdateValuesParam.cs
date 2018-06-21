using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Web.Api.Entities
{
    public class UpdateValuesParam
    {
        public IEnumerable<EditItem> EditItems { get; set; }

        public CostEditorContext Context { get; set; }
    }
}
