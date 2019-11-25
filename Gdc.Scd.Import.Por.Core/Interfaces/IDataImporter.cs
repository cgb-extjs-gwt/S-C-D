using System.Linq;

namespace Gdc.Scd.Import.Por.Core.Interfaces
{
    public interface IDataImporter<out T> where T : class
    {
        IQueryable<T> ImportData();
    }
}
