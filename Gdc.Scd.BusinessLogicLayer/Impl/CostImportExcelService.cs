using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CostImportExcelService : ICostImportExcelService
    {
        private readonly ICostBlockService costBlockService;

        private readonly ISqlRepository sqlRepository;

        private readonly DomainEnitiesMeta metas;

        public CostImportExcelService(
            ICostBlockService costBlockService,
            ISqlRepository sqlRepository,
            DomainEnitiesMeta metas)
        {
            this.costBlockService = costBlockService;
            this.sqlRepository = sqlRepository;
            this.metas = metas;
        }

        public async Task<ExcelImportResult> Import(CostImportContext context, Stream excelStream, ApprovalOption approvalOption)
        {
            var result = new ExcelImportResult
            {
                Errors = new List<string>()
            };

            try
            {
                var workbook = new XLWorkbook(excelStream);
                var worksheetInfo = 
                    workbook.Worksheets.Select(worksheet => new
                                        {
                                            Worksheet = worksheet,
                                            RowsUsed = worksheet.RowsUsed()
                                        })
                                       .FirstOrDefault(info => info.RowsUsed.Any());

                if (worksheetInfo == null)
                {
                    result.Errors.Add("Excel file is empty");
                }
                else
                {
                    var rawValues = new Dictionary<string, string>();

                    foreach (var row in worksheetInfo.RowsUsed)
                    {
                        var rowIndex = row.RowNumber();
                        var name = worksheetInfo.Worksheet.Cell(rowIndex, 1).GetValue<string>();

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            var value = worksheetInfo.Worksheet.Cell(rowIndex, 2).GetValue<string>();

                            rawValues[name] = !string.IsNullOrWhiteSpace(value) ? value : null;
                        }
                    }

                    var editInfoResult = await this.BuildEditInfos(context, rawValues);

                    result.Errors.AddRange(editInfoResult.Errors);

                    if (editInfoResult.EditInfo.ValueInfos.Any())
                    {
                        var qualityGateResultSet = await this.costBlockService.Update(new[] { editInfoResult.EditInfo }, approvalOption, EditorType.CostImport);
                        var qualityGateResultSetItem = qualityGateResultSet.Items.FirstOrDefault();

                        if (qualityGateResultSetItem != null)
                        {
                            result.QualityGateResult = qualityGateResultSetItem.QualityGateResult;
                        }
                    }
                    else
                    {
                        result.Errors.Add($"Worksheet '{worksheetInfo.Worksheet.Name}' has not available items");
                    }
                }
            }
            catch(Exception ex)
            {
                result.Errors.Add($"Import error. {ex.Message}");
            }

            if (result.QualityGateResult == null)
            {
                result.QualityGateResult = new QualityGateResult();
            }

            return result;
        }

        private async Task<(EditInfo EditInfo, IEnumerable<string> Errors)> BuildEditInfos(CostImportContext context, IDictionary<string, string> rawValues)
        {
            var errors = new List<string>();

            var costBlockMeta = this.metas.GetCostBlockEntityMeta(context);
            var inputLevelField = costBlockMeta.InputLevelFields[context.InputLevelId];

            var inputLevelItems =
                    await this.sqlRepository.GetNameIdItems(
                        inputLevelField.ReferenceMeta, 
                        inputLevelField.ReferenceValueField, 
                        inputLevelField.ReferenceFaceField);

            var inputLevelItemsDictionary = inputLevelItems.ToDictionary(item => item.Name);

            var converter = await this.BuildConverter(costBlockMeta, context.CostElementId);
            var valueInfos = new List<ValuesInfo>();
            var dependencyFilter = this.BuildFilter(costBlockMeta, context);

            foreach (var rawValue in rawValues)
            {
                try
                {
                    if (inputLevelItemsDictionary.TryGetValue(rawValue.Key, out var inputLevelItem))
                    {
                        valueInfos.Add(new ValuesInfo
                        {
                            CoordinateFilter = new Dictionary<string, long[]>(dependencyFilter)
                            {
                                [inputLevelField.Name] = new[] {inputLevelItem.Id}
                            },
                            Values = new Dictionary<string, object>
                            {
                                [context.CostElementId] = rawValue.Value != null ? converter(rawValue.Value) : null
                            }
                        });
                    }
                    else
                    {
                        errors.Add($"{inputLevelField.ReferenceMeta.Name} '{rawValue.Key}' not found");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Import error - {inputLevelField.ReferenceMeta.Name} '{rawValue.Key}', value '{rawValue.Value}'. {ex.Message}");
                }
            }

            var editInfo = new EditInfo
            {
                Meta = costBlockMeta,
                ValueInfos = valueInfos
            };

            return (editInfo, errors);
        }

        private async Task<Func<string, object>> BuildConverter(CostBlockEntityMeta meta, string costElementId)
        {
            Func<string, object> converter;

            switch (meta.CostElementsFields[costElementId])
            {
                case SimpleFieldMeta simpleField:
                    switch(simpleField.Type)
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

        private IDictionary<string, long[]> BuildFilter(CostBlockEntityMeta costBlockMeta, CostImportContext context)
        {
            var filter = new Dictionary<string, long[]>();
            var costElement = costBlockMeta.SliceDomainMeta.CostElements[context.CostElementId];

            if (context.DependencyItemId.HasValue)
            {
                if (costElement.Dependency == null)
                {
                    throw new Exception($"Cost element '{context.CostElementId}' has not dependency, but parameter 'dependencyItemId' has value");
                }
                else
                {
                    filter.Add(costElement.Dependency.Id, new [] { context.DependencyItemId.Value });
                }
            }

            if (context.RegionId.HasValue)
            {
                if (costElement.RegionInput == null)
                {
                    throw new Exception($"Cost element '{context.CostElementId}' has not region, but parameter 'regionId' has value");
                }
                else
                {
                    filter.Add(costElement.RegionInput.Id, new[] { context.RegionId.Value });
                }
            }

            return filter;
        }
    }
}
