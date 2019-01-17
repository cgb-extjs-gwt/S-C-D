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

        public async Task<ExcelImportResult> Import(ICostElementIdentifier costElementId, Stream excelStream, long? dependencyItemId = null)
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

                await this.costBlockRepository.Update(editInfoResult.EditInfos);
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
            var dependencyFilter = new Dictionary<string, IEnumerable<object>>();

            if (dependencyItemId.HasValue)
            {
                var dependencyMeta = costBlockMeta.DomainMeta.CostElements[costElementId.CostElementId].Dependency;

                if (dependencyMeta == null)
                {
                    throw new Exception($"Cost element '{costElementId.CostElementId}' has not dependency, but parameter 'dependencyItemId' has value");
                }
                else
                {
                    dependencyFilter.Add(dependencyMeta.Id, new object[] { dependencyItemId.Value });
                }
            }

            var costElementField = costBlockMeta.CostElementsFields[costElementId.CostElementId];

            foreach (var wgValue in wgRawValues)
            {
                try
                {
                    editInfos.Add(new EditInfo
                    {
                        Meta = costBlockMeta,
                        ValueInfos = new[]
                        {
                                new ValuesInfo
                                {
                                    Filter = new Dictionary<string, IEnumerable<object>>(dependencyFilter)
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
                    errors.Add($"Import error - warranty group '{wgValue.Key}', value '{wgValue.Value}'");
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

                    converter = rawValue => referenceItemsDict[rawValue.ToUpper()];
                    break;

                default:
                    throw new NotSupportedException("Cost element field type not supported");
            }

            return converter;
        }
    }
}
