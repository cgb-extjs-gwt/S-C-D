using System.Collections.Generic;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class UpdateQueryOption
    {
        public Dictionary<string, long> OldCoordinates { get; private set; }
        public Dictionary<string, long> NewCoordinates { get; private set; }

        public UpdateQueryOption(Dictionary<string, long> oldCoordinates, 
            Dictionary<string, long> newCoordinates)
        {
            OldCoordinates = oldCoordinates;
            NewCoordinates = newCoordinates;
        }
    }
}
