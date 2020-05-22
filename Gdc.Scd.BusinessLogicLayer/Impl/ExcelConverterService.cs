using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
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
                            converter = rawValue => this.ConvertToBool(rawValue);
                            break;

                        default:
                            converter = rawValue =>
                            {
                                object result;

                                if (string.IsNullOrWhiteSpace(rawValue))
                                {
                                    result = null;
                                }
                                else
                                {
                                    result = Convert.ChangeType(rawValue, simpleField.Type);
                                }

                                return result;
                            };
                            break;
                    }
                    break;

                case ReferenceFieldMeta referenceField:
                    var refConverter = await this.BuildReferenceConverter(referenceField);

                    converter = rawValue => refConverter(rawValue)?.Id;
                    break;

                default:
                    throw new NotSupportedException("Cost element field type not supported");
            }

            return converter;
        }

        public async Task<Func<string, NamedId>> BuildReferenceConverter(BaseEntityMeta meta, string valueField, string faceField)
        {
            var referenceItems = await this.sqlRepository.GetNameIdItems(meta,valueField, faceField);
            var referenceItemsDict = referenceItems.ToDictionary(item => item.Name.ToUpper());

            return rawValue =>
            {
                NamedId item;

                if (string.IsNullOrWhiteSpace(rawValue)) 
                {
                    item = null;
                }
                else if (!referenceItemsDict.TryGetValue(rawValue.ToUpper(), out item))
                {
                    throw new Exception($"'{rawValue}' not found in {meta.Name}");
                }

                return item;
            };
        }

        public async Task<Func<string, NamedId>> BuildReferenceConverter(ReferenceFieldMeta referenceField)
        {
            return await this.BuildReferenceConverter(
                referenceField.ReferenceMeta,
                referenceField.ReferenceValueField,
                referenceField.ReferenceFaceField);
        }

        public async Task<Func<string, NamedId>> BuildReferenceConverter(NamedEntityMeta meta)
        {
            return await this.BuildReferenceConverter(meta, meta.IdField.Name, meta.NameField.Name);
        }

        public bool ConvertToBool(string rawValue)
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
        }
    }
}
