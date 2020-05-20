using Gdc.Scd.Core.Meta.Entities;
using System;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IExcelConverterService
    {
        Task<Func<string, object>> BuildConverter(CostBlockEntityMeta meta, string costElementId);
    }
}