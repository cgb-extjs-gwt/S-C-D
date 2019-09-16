using System;

namespace Gdc.Scd.Core.Entities.Portfolio
{
    public abstract class BasePortfolioInheritance
    {
        public long PlaId { get; set; }

        public long AvailabilityId { get; set; }

        public long DurationId { get; set; }

        public long ReactionTypeId { get; set; }

        public long ReactionTimeId { get; set; }

        public long ServiceLocationId { get; set; }

        public long ProActiveSlaId { get; set; }

        public void Set(string fieldName, object value)
        {
            var property = this.GetType().GetProperty(fieldName);
            if (property == null)
            {
                throw new Exception($"Property '{fieldName}' not found");
            }

            property.SetValue(this, value);
        }
    }
}
