using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostElementExcelService : ICostElementExcelService
    {
        private readonly ICostBlockRepository costBlockRepository;

        private readonly IDomainService<Wg> wgService;

        private readonly ISqlRepository sqlRepository;

        private readonly DomainEnitiesMeta metas;

        public CostElementExcelService(
            ICostBlockRepository costBlockRepository, 
            IDomainService<Wg> wgService,
            ISqlRepository sqlRepository,
            DomainEnitiesMeta metas)
        {
            this.costBlockRepository = costBlockRepository;
            this.wgService = wgService;
            this.sqlRepository = sqlRepository;
            this.metas = metas;
        }

        public async Task<ExcelImportResult> Import(ICostElementIdentifier costElementId, Stream excelStream)
        {
            var result = new ExcelImportResult
            {
                Errors = new List<string>()
            };

            try
            {
                var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.First();
                var rowCount = worksheet.RowCount();
                var wgRawValues = new Dictionary<string, string>();

                for (var rowIndex = 1; rowIndex <= rowCount; rowIndex++)
                {
                    var wgName = worksheet.Cell(rowIndex, 1).GetValue<string>();

                    if (string.IsNullOrWhiteSpace(wgName))
                    {
                        var wgValue = worksheet.Cell(rowIndex, 2).GetValue<string>();

                        if (string.IsNullOrWhiteSpace(wgValue))
                        {
                            wgRawValues[wgName] = wgValue;
                        }
                    }
                }

                var wgs =
                    this.wgService.GetAll()
                                  .Where(wg => wgRawValues.Keys.Contains(wg.Name))
                                  .ToDictionary(wg => wg.Name);

                var costBlockMeta = this.metas.GetCostBlockEntityMeta(costElementId);
                var converter = await this.BuildConverter(costBlockMeta, costElementId.CostElementId);
                var editInfos = new List<EditInfo>();

                foreach (var wgValue in wgRawValues)
                {
                    var costElementField = costBlockMeta.CostElementsFields[costElementId.CostElementId];

                    try
                    {
                        editInfos.Add(new EditInfo
                        {
                            Meta = costBlockMeta,
                            ValueInfos = new[]
                            {
                            new ValuesInfo
                            {
                                Filter = new Dictionary<string, IEnumerable<object>>
                                {
                                    [MetaConstants.WgInputLevelName] = new object[] { wgs[wgValue.Key].Id }
                                },
                                Values = new Dictionary<string, object>
                                {
                                    [costElementId.CostElementId] = converter(wgValue.Value)
                                }
                            }
                        }
                        });
                    }
                    catch
                    {
                        result.Errors.Add($"Import error - warranty group '{wgValue.Key}', value '{wgValue.Value}'");
                    }
                }

                await this.costBlockRepository.Update(editInfos);
            }
            catch(Exception ex)
            {
                result.Errors.Add($"Import error. {ex.Message}");
            }

            return result;
        }

        private async Task<Func<string, object>> BuildConverter(CostBlockEntityMeta meta, string costElementId)
        {
            Func<string, object> converter;

            switch (meta.CostElementsFields[costElementId])
            {
                case SimpleFieldMeta simpleField:
                    converter = rawValue => Convert.ChangeType(rawValue, simpleField.Type);
                    break;

                case ReferenceFieldMeta referenceField:
                    var referenceItems = 
                        await this.sqlRepository.GetNameIdItems(
                            referenceField.ReferenceMeta, 
                            referenceField.ReferenceValueField, 
                            referenceField.ReferenceFaceField);

                    var referenceItemsDict = referenceItems.ToDictionary(item => item.Name, item => item.Id);

                    converter = rawValue => referenceItemsDict[rawValue];
                    break;

                default:
                    throw new NotSupportedException("Cost element field type not supported");
            }

            return converter;
        }
    }
}
