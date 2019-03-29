using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IDataAccessManager
    {
        IEnumerable<T> ExecuteQuery<T>(DbCommand command) where T : new();
    }
}
