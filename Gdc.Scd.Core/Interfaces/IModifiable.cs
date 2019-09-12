using System;

namespace Gdc.Scd.Core.Interfaces
{
    public interface IModifiable : IDeactivatable
    {
        DateTime ModifiedDateTime { get; set; }
    }
}
