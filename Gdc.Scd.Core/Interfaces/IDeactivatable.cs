using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Core.Interfaces
{
    public interface IDeactivatable
    {
        DateTime CreatedDateTime { get; set; }
        DateTime? DeactivatedDateTime { get; set; }
        DateTime ModifiedDateTime { get; set; }
    }
}
