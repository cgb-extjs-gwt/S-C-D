using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.CopyDataTool.Entities
{
    public class Cache
    {
        //key - cost element name
        //value - Dictionary: key - concatenated coordinates, value - cost element value
        public Dictionary<string, Dictionary<string, object>> ApprovedCache { get; set; }

        //key - cost element name
        //value - Dictionary: key - concatenated coordinates, value - cost element value
        public Dictionary<string, Dictionary<string, object>> ToBeApprovedCache { get; set; }

        public Cache()
        {
            ApprovedCache = new Dictionary<string, Dictionary<string, object>>();
            ToBeApprovedCache = new Dictionary<string, Dictionary<string, object>>();
        }

        public bool IfExists(string costElementName, Dictionary<string, long> coordinates, 
            object value, ApproveSet approveSet)
        {
            var cache = approveSet == ApproveSet.Approved ? ApprovedCache : ToBeApprovedCache;

            var coordinateKey = GenerateCoordinateKey(coordinates);

            return cache.ContainsKey(costElementName) &&
                   cache[costElementName].ContainsKey(coordinateKey) &&
                   cache[costElementName][coordinateKey]?.ToString() == value?.ToString();
        }


        public bool Add(string costElementName, Dictionary<string, long> coordinates,
            object value, ApproveSet approveSet)
        {
            var cache = approveSet == ApproveSet.Approved ? ApprovedCache : ToBeApprovedCache;

            var coordinateKey = GenerateCoordinateKey(coordinates);
            if (!cache.ContainsKey(costElementName))
                cache[costElementName] = new Dictionary<string, object>();

            if (!cache[costElementName].ContainsKey(coordinateKey))
            {
                cache[costElementName][coordinateKey] = value;
                return true;
            }

            return false;
        }

        private string GenerateCoordinateKey(Dictionary<string, long> coordinates)
        {
            return String.Join(",", coordinates.Values);
        }
    }

    public enum ApproveSet
    {
        Approved,
        ToBeApproved
    }
}
