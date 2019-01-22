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
    public class CostImportExcelService : ICostImportExcelService
    {
        private readonly ICostBlockService costBlockService;

        private readonly IDomainService<Wg> wgService;

        private readonly ISqlRepository sqlRepository;

        private readonly DomainEnitiesMeta metas;

        public CostImportExcelService(
            ICostBlockService costBlockService,
            IDomainService<Wg> wgService,
            ISqlRepository sqlRepository,
            DomainEnitiesMeta metas)
        {
            this.costBlockService = costBlockService;
            this.wgService = wgService;
            this.sqlRepository = sqlRepository;
            this.metas = metas;
        }

        public async Task<ExcelImportResult> Import(ICostElementIdentifier costElementId, Stream excelStream, ApprovalOption approvalOption, long? dependencyItemId = null)
        {
            var result = new ExcelImportResult
            {
                Errors = new List<string>()
            };

            try
            {
                var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheets.First();
                var rowCount = worksheet.RowsUsed().Count();
                var wgRawValues = new Dictionary<string, string>();

                for (var rowIndex = 1; rowIndex <= rowCount; rowIndex++)
                {
                    var wgName = worksheet.Cell(rowIndex, 1).GetValue<string>();

                    if (!string.IsNullOrWhiteSpace(wgName))
                    {
                        var wgValue = worksheet.Cell(rowIndex, 2).GetValue<string>();

                        if (!string.IsNullOrWhiteSpace(wgValue))
                        {
                            wgRawValues[wgName] = wgValue;
                        }
                    }
                }

                var editInfoResult = await this.BuildEditInfos(costElementId, dependencyItemId, wgRawValues);

                result.Errors.AddRange(editInfoResult.Errors);

                var qualityGateResultSet = await this.costBlockService.Update(editInfoResult.EditInfos.ToArray(), approvalOption, EditorType.CostImport);
                var qualityGateResultSetItem = qualityGateResultSet.Items.FirstOrDefault();

                result.QualityGateResult = qualityGateResultSetItem == null
                    ? new QualityGateResult()
                    : qualityGateResultSetItem.QualityGateResult;
            }
            catch(Exception ex)
            {
                result.Errors.Add($"Import error. {ex.Message}");
            }

            return result;
        }

        private async Task<(IEnumerable<EditInfo> EditInfos, IEnumerable<string> Errors)> BuildEditInfos(
            ICostElementIdentifier costElementId,
            long? dependencyItemId,
            IDictionary<string, string> wgRawValues)
        {
            var errors = new List<string>();
            var wgs =
                this.wgService.GetAll()
                              .Where(wg => wgRawValues.Keys.Contains(wg.Name))
                              .ToDictionary(wg => wg.Name);

            var costBlockMeta = this.metas.GetCostBlockEntityMeta(costElementId);
            var converter = await this.BuildConverter(costBlockMeta, costElementId.CostElementId);
            var editInfos = new List<EditInfo>();
            var dependencyFilter = this.BuildDependencyFilter(costBlockMeta, costElementId, dependencyItemId);
            var costElementField = costBlockMeta.CostElementsFields[costElementId.CostElementId];

            foreach (var wgValue in wgRawValues)
            {
                try
                {
                    if (wgs.TryGetValue(wgValue.Key, out var wg))
                    {
                        editInfos.Add(new EditInfo
                        {
                            Meta = costBlockMeta,
                            ValueInfos = new[]
                            {
                                new ValuesInfo
                                {
                                    CoordinateFilter = new Dictionary<string, long[]>(dependencyFilter)
                                    {
                                        [MetaConstants.WgInputLevelName] = new [] { wg.Id }
                                    },
                                    Values = new Dictionary<string, object>
                                    {
                                        [costElementId.CostElementId] = converter(wgValue.Value)
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        errors.Add($"Warranty group '{wgValue.Key}' not found");
                    }
                }
                catch(Exception ex)
                {
                    errors.Add($"Import error - warranty group '{wgValue.Key}', value '{wgValue.Value}'. {ex.Message}");
                }
            }

            return (editInfos, errors);
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

        private IDictionary<string, long[]> BuildDependencyFilter(
            CostBlockEntityMeta costBlockMeta,
            ICostElementIdentifier costElementId,
            long? dependencyItemId)
        {
            var dependencyFilter = new Dictionary<string, long[]>();

            if (dependencyItemId.HasValue)
            {
                var dependencyMeta = costBlockMeta.DomainMeta.CostElements[costElementId.CostElementId].Dependency;

                if (dependencyMeta == null)
                {
                    throw new Exception($"Cost element '{costElementId.CostElementId}' has not dependency, but parameter 'dependencyItemId' has value");
                }
                else
                {
                    dependencyFilter.Add(dependencyMeta.Id, new [] { dependencyItemId.Value });
                }
            }

            return dependencyFilter;
        }
    }
}
