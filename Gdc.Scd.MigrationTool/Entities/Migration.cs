using System;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.MigrationTool.Entities
{
    public class Migration : IIdentifiable
    {
        public long Id { get; set; }

        public int Number { get; set; }

        public DateTime ExecutionDate { get; set; }

        public string Description { get; set; }
    }
}
