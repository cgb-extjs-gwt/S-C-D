using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core.Interfaces
{
    public interface IParser<out T>
    {
        IEnumerable<T> Parse(ParseInfoDto info);
    }
}
