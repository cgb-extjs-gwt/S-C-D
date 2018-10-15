using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IUploader<in T>
    {
        void Upload(IEnumerable<T> items, DateTime modifiedDateTime);
        int Deactivate(DateTime modifiedDateTime);
    }
}
