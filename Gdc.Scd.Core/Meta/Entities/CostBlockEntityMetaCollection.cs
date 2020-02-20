using Gdc.Scd.Core.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockEntityMetaCollection : IEnumerable<CostBlockEntityMeta>
    {
        private readonly MetaCollection<CostBlockEntityMeta> costBlocks;
        private readonly Dictionary<CostElementIdentifierKey, CostBlockEntityMeta> costElementIdMapping;
        private readonly Dictionary<CostBlockEntityMeta, Dictionary<string, CostElementIdentifierKey>> costBlockMapping;

        public CostBlockEntityMeta this[CostElementIdentifierKey key]
        {
            get 
            {
                return this.costElementIdMapping[key];
            }
        }

        public CostBlockEntityMeta this[string applicationId, string costBlockId, string costElementId]
        {
            get
            {
                return this[new CostElementIdentifierKey(applicationId, costBlockId, costElementId)];
            }
        }

        public CostBlockEntityMeta this[ICostElementIdentifier id]
        {
            get
            {
                return this[id.ApplicationId, id.CostBlockId, id.CostElementId];
            }
        }

        public CostBlockEntityMeta this[ICostBlockIdentifier costBlockId, string costElementId]
        {
            get
            {
                return this[costBlockId.ApplicationId, costBlockId.CostBlockId, costElementId];
            }
        }

        public CostBlockEntityMeta this[string fullName]
        {
            get
            {
                return this.costBlocks[fullName];
            }
        }

        public CostBlockEntityMeta this[string shema, string tableName]
        {
            get
            {
                return this[BaseEntityMeta.BuildFullName(tableName, shema)];
            }
        }

        public CostBlockEntityMetaCollection()
        {
            this.costBlocks = new MetaCollection<CostBlockEntityMeta>();
            this.costElementIdMapping = new Dictionary<CostElementIdentifierKey, CostBlockEntityMeta>();
            this.costBlockMapping = new Dictionary<CostBlockEntityMeta, Dictionary<string, CostElementIdentifierKey>>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<CostBlockEntityMeta> GetEnumerator()
        {
            return this.costBlocks.GetEnumerator();
        }

        public void Add(CostElementIdentifierKey key, CostBlockEntityMeta meta)
        {
            this.costElementIdMapping.Add(key, meta);

            if (!this.costBlocks.Contains(meta))
            {
                this.costBlocks.Add(meta);
            }

            if (!this.costBlockMapping.TryGetValue(meta, out var keys))
            {
                keys = new Dictionary<string, CostElementIdentifierKey>();

                this.costBlockMapping.Add(meta, keys);
            }

            keys.Add(key.CostElementId, key);
        }

        public void Add(IEnumerable<CostElementIdentifierKey> keys, CostBlockEntityMeta meta)
        {
            foreach (var key in keys)
            {
                this.Add(key, meta);
            }
        }

        public void Remove(CostBlockEntityMeta meta)
        {
            this.costBlocks.Remove(meta);

            var deleteKeys =
                this.costElementIdMapping.Where(keyValue => keyValue.Value == meta)
                                         .Select(keyValue => keyValue.Key);

            foreach (var key in deleteKeys)
            {
                this.costElementIdMapping.Remove(key);
            }

            this.costBlockMapping.Remove(meta);
        }

        public IEnumerable<CostElementIdentifierKey> GetCostElementIdentifiers(CostBlockEntityMeta costBlock)
        {
            return this.costBlockMapping[costBlock].Values;
        }

        public CostElementIdentifierKey GetCostElementIdentifier(CostBlockEntityMeta costBlock, string costElementId)
        {
            return this.costBlockMapping[costBlock][costElementId];
        }
    }
}
