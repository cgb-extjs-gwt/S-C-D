using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Meta.Entities
{
    public struct CostElementIdentifierKey : ICostElementIdentifier
    {
        public string CostElementId { get; }

        public string ApplicationId { get; }

        public string CostBlockId { get; }

        public CostElementIdentifierKey(string applicationId, string costBlockId, string costElementId)
        {
            this.ApplicationId = applicationId;
            this.CostBlockId = costBlockId;
            this.CostElementId = costElementId;
        }

        public CostElementIdentifierKey(ICostElementIdentifier costElementId)
            : this(costElementId.ApplicationId, costElementId.CostBlockId, costElementId.CostElementId)
        { 
        }

        public CostElementIdentifierKey(ICostBlockIdentifier costBlockId, string costElementId)
            : this(costBlockId.ApplicationId, costBlockId.CostBlockId, costElementId)
        {
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

            if (!result && obj is CostElementIdentifierKey)
            {
                var costElementId = (CostElementIdentifierKey)obj;

                result =
                    this.ApplicationId == costElementId.ApplicationId &&
                    this.CostBlockId == costElementId.CostBlockId &&
                    this.CostElementId == costElementId.CostElementId;
            }

            return result;
        }
    }
}
