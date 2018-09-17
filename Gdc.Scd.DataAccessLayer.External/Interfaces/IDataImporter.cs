using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.External.Interfaces
{
    public interface IDataImporter<out T> where T: class
    {
        IEnumerable<T> ImportData();
    }
}
