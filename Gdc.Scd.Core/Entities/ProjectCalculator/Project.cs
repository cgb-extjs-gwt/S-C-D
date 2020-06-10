using Gdc.Scd.Core.Meta.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gdc.Scd.Core.Entities.ProjectCalculator
{
    [Table("Project", Schema = MetaConstants.ProjectCalculatorSchema)]
    public class Project : NamedId
    {
        public User User { get; set; }

        public long UserId { get; set; }

        public DateTime CreationDate { get; set; }

        public List<ProjectItem> ProjectItems { get; set; }
    }
}
