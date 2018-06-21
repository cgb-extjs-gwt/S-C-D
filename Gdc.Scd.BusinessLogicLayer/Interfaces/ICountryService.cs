using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface ICountryService
    {
        Task<IEnumerable<string>> GetAll();
    }
}
