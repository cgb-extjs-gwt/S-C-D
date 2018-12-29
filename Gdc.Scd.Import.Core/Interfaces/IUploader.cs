using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IUploader<in T>
    {
        IEnumerable<UpdateQueryOption> Upload(IEnumerable<T> items, DateTime modifiedDateTime);
    }
}
