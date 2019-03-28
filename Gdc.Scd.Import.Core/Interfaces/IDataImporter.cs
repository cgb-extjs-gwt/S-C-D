using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IDataImporter<T>
    {
        IEnumerable<T> ImportData();
    }
}
