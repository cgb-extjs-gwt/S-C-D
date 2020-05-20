using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ExcelConverterService : IExcelConverterService
    {
        private readonly ISqlRepository sqlRepository;

        public ExcelConverterService(ISqlRepository sqlRepository)
        {
            this.sqlRepository = sqlRepository;
        }

        public async Task<Func<string, object>> BuildConverter(CostBlockEntityMeta meta, string costElementId)
        {
            Func<string, object> converter;

            switch (meta.CostElementsFields[costElementId])
            {
                case SimpleFieldMeta simpleField:
                    switch (simpleField.Type)
                    {
                        case TypeCode.Boolean:
                            converter = rawValue =>
                            {
                                bool result;

                                var rawValueUpper = rawValue.ToUpper();

                                if (rawValueUpper == "0" || rawValueUpper == "FALSE")
                                {
                                    result = false;
                                }
                                else if (rawValueUpper == "1" || rawValueUpper == "TRUE")
                                {
                                    result = true;
                                }
                                else
                                {
                                    throw new Exception($"Unable to convert value from '{rawValue}' to boolean");
                                }

                                return result;
                            };
                            break;

                        default:
                            converter = rawValue => Convert.ChangeType(rawValue, simpleField.Type);
                            break;
                    }
                    break;

                case ReferenceFieldMeta referenceField:
                    var referenceItems =
                        await this.sqlRepository.GetNameIdItems(
                            referenceField.ReferenceMeta,
                            referenceField.ReferenceValueField,
                            referenceField.ReferenceFaceField);

                    var referenceItemsDict = referenceItems.ToDictionary(item => item.Name.ToUpper(), item => item.Id);

                    converter = rawValue =>
                    {
                        if (!referenceItemsDict.TryGetValue(rawValue.ToUpper(), out var id))
                        {
                            throw new Exception($"'{rawValue}' not found in {referenceField.Name}");
                        }

                        return id;
                    };
                    break;

                default:
                    throw new NotSupportedException("Cost element field type not supported");
            }

            return converter;
        }
    }
}
