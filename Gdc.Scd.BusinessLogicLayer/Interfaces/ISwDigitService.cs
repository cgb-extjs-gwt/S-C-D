using Gdc.Scd.Core.Entities;
using System.Linq;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ISwDigitService : IDomainService<SwDigit>
    {
        IQueryable<Sog> GetDigitSog();
    }
}
