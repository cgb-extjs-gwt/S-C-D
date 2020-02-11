using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class CostElementIdentifier : ICostElementIdentifier
    {
        public string ApplicationId { get; set; }

        public string CostElementId { get; set; }

        public string CostBlockId { get; set; }

        public CostElementIdentifier()
        { 
        }

        public CostElementIdentifier(string applicationId, string costBlockId, string costElementId)
        {
            this.ApplicationId = applicationId;
            this.CostBlockId = costBlockId;
            this.CostElementId = costElementId;
        }

        public override int GetHashCode()
        {
            unchecked 
            {
                var hash = 17;
                
                hash = hash * 23 + this.ApplicationId.GetHashCode();
                hash = hash * 23 + this.CostBlockId.GetHashCode();
                hash = hash * 23 + this.CostElementId.GetHashCode();

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            var result = object.ReferenceEquals(this, obj);

            if (!result && obj != null)
            {
                var costElementId = obj as CostElementIdentifier;
                if (costElementId != null)
                {
                    result =
                        this.ApplicationId == costElementId.ApplicationId &&
                        this.CostBlockId == costElementId.CostBlockId &&
                        this.CostElementId == costElementId.CostElementId;
                }
            }

            return result;
        }
    }
}
