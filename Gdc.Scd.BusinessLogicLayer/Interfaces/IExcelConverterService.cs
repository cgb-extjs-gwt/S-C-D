using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Entities;
using System;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface IExcelConverterService
    {
        Task<Func<string, object>> BuildConverter(CostBlockEntityMeta meta, string costElementId);

        Task<Func<string, NamedId>> BuildReferenceConverter(BaseEntityMeta meta, string valueField, string faceField);

        Task<Func<string, NamedId>> BuildReferenceConverter(ReferenceFieldMeta referenceField);

        Task<Func<string, NamedId>> BuildReferenceConverter(NamedEntityMeta meta);

        bool ConvertToBool(string rawValue);
    }
}