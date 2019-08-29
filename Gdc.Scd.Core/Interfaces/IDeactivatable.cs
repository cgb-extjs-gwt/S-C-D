using System;

namespace Gdc.Scd.Core.Interfaces
{
    public interface IDeactivatable
    {
        DateTime CreatedDateTime { get; set; }

        DateTime? DeactivatedDateTime { get; set; }
    }
}
