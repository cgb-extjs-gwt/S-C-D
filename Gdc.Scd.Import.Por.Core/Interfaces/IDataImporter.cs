using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IDataImporter<out T> where T : class
    {
        IQueryable<T> ImportData();
    }
}
