using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.MigrationTool.Entities;
using Gdc.Scd.MigrationTool.Impl;
using Gdc.Scd.MigrationTool.Interfaces;

namespace Gdc.Scd.MigrationTool.Migrations
{
    public class Migration_2019_03_28_14_54 //: IMigrationAction
    {
        private readonly ICostBlockService costBlockService;

        private readonly IApprovalService approvalService;

        private readonly ISqlRepository sqlRepository;

        private readonly DomainEnitiesMeta meta;

        private readonly Dictionary<string, string> nameMapping = new Dictionary<string, string>
        {
            ["Digit (SW licence)".ToUpper()] = "SW Digit".ToUpper(),
            ["Discount to Dealer Price".ToUpper()] = "Discount to Dealer price in %".ToUpper(),
            ["9x5 - 1 years".ToUpper()] = "1 Year 9x5".ToUpper(),
            ["9x5 - 2 years".ToUpper()] = "2 Years 9x5".ToUpper(),
            ["9x5 - 3 years".ToUpper()] = "3 Years 9x5".ToUpper(),
            ["9x5 - 4 years".ToUpper()] = "4 Years 9x5".ToUpper(),
            ["9x5 - 5 years".ToUpper()] = "5 Years 9x5".ToUpper(),
            ["9x5 - Prolongation".ToUpper()] = "Prolongation 9x5".ToUpper(),
            ["24x7 - 1 years".ToUpper()] = "1 Year 24x7".ToUpper(),
            ["24x7 - 2 years".ToUpper()] = "2 Years 24x7".ToUpper(),
            ["24x7 - 3 years".ToUpper()] = "3 Years 24x7".ToUpper(),
            ["24x7 - 4 years".ToUpper()] = "4 Years 24x7".ToUpper(),
            ["24x7 - 5 years".ToUpper()] = "5 Years 24x7".ToUpper(),
            ["24x7 - Prolongation".ToUpper()] = "Prolongation 24x7".ToUpper(),
            ["1 Years 9x5".ToUpper()] = "1 Year 9x5".ToUpper(),
            ["1 Years 24x7".ToUpper()] = "1 Year 24x7".ToUpper(),
            ["€".ToUpper()] = "EUR".ToUpper(),
        };

        public int Number => 77777; //!!!!!!!!!!!!!!

        public string Description => "Import from excel files";

        public Migration_2019_03_28_14_54(
            ICostBlockService costBlockService,
            IApprovalService approvalService,
            ISqlRepository sqlRepository, 
            DomainEnitiesMeta meta)
        {
            this.costBlockService = costBlockService;
            this.approvalService = approvalService;
            this.sqlRepository = sqlRepository;
            this.meta = meta;
        }

        public void Execute()
        {
            PrincipalProvider.CurrentPricipal = new Principal
            {
                Identity = new Identity
                {
                    Name = @"G02\MCHSBach",
                    IsAuthenticated = true
                }
            };

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var folderName = $"{assemblyName.Name}.MigrationFiles.Migration_2019_03_28_14_54";

            var fileNames = new[]
            {
                $"{folderName}.2019-03-28 CS200c SCD2-0 List (sb).xlsx",
                $"{folderName}.2019-03-28 ESM SCD2-0 List (sb).xlsx",
                $"{folderName}.Import Maschlanka SDC V2.0.xlsx",
                //$"{folderName}.test import.xlsx"
            };

            var costBlocks = this.meta.CostBlocks.Where(costBlock => costBlock.Schema == MetaConstants.SoftwareSolutionSchema).ToArray();

            var editInfos = new List<EditInfo>();

            Console.WriteLine("Analizing excels...");

            foreach (var fileName in fileNames)
            {
                using (var stream = assembly.GetManifestResourceStream(fileName))
                {
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var editInfosTask = this.GetEditInfos(worksheet, costBlocks);

                        editInfosTask.Wait();

                        editInfos.AddRange(editInfosTask.Result);
                    }
                }
            }

            Console.WriteLine("Updating data...");

            var updateTask =
                this.costBlockService.UpdateWithoutQualityGate(
                    editInfos.ToArray(),
                    new ApprovalOption { IsApproving = true },
                    EditorType.Migration);

            updateTask.Wait();

            var histories = updateTask.Result.ToList();

            Console.WriteLine("Approving...");
            Console.WriteLine($"Approving history count - {histories.Count}");

            var historyIndex = 0;

            foreach (var history in histories)
            {
                Console.WriteLine($"History index: {historyIndex++}/{histories.Count - 1}");

                var approveTask = this.approvalService.Approve(history.Id);

                approveTask.Wait();
            }

            Console.WriteLine("Data imported");
        }

        private async Task<IEnumerable<EditInfo>> GetEditInfos(IXLWorksheet worksheet, CostBlockEntityMeta[] costBlocks)
        {
            var result = new List<EditInfo>();
            var excelColumnInfo = await this.BuildExcelColumsInfo(worksheet, costBlocks);
            var inputLevelItemsCache = new Dictionary<string, Dictionary<string, NamedId>>();
            var columnCount = worksheet.ColumnsUsed().Count();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var inputLevelIds = new Dictionary<string, long>();
                var editInfos = new Dictionary<CostBlockEntityMeta, EditInfo>();

                for (var columnIndex = 1; columnIndex <= columnCount; columnIndex++)
                {
                    var cell = row.Cell(columnIndex);

                    if (excelColumnInfo.InputLevelInfos.TryGetValue(columnIndex, out var inputLevelMeta))
                    {
                        var inputLevelItemName = cell.GetValue<string>();
                        var inputLevelItem = await this.GetNamedId(inputLevelMeta, inputLevelItemName, inputLevelItemsCache);

                        inputLevelIds.Add(inputLevelMeta.Name, inputLevelItem.Id);
                    }
                    else
                    {
                        var costElementInfo = excelColumnInfo.CostElementsInfos[columnIndex];

                        if (!editInfos.TryGetValue(costElementInfo.CostBlockMeta, out var editInfo))
                        {
                            editInfo = new EditInfo
                            {
                                Meta = costElementInfo.CostBlockMeta,
                                ValueInfos = Enumerable.Empty<ValuesInfo>()
                            };

                            editInfos[costElementInfo.CostBlockMeta] = editInfo;
                        }

                        var coordinateFilter = new Dictionary<string, long[]>();

                        if (costElementInfo.DependencyItemId.HasValue)
                        {
                            coordinateFilter[costElementInfo.CostElement.Dependency.Id] = new[] { costElementInfo.DependencyItemId.Value };
                        }

                        editInfo.ValueInfos = editInfo.ValueInfos.Concat(new[]
                        {
                            new ValuesInfo
                            {
                                CoordinateFilter = coordinateFilter,
                                Values = new Dictionary<string, object>
                                {
                                    [costElementInfo.CostElement.Id] = GetCostElementValue(cell, costElementInfo)
                                }
                            }
                        });
                    }
                }

                foreach(var editInfo in editInfos.Values)
                {
                    editInfo.ValueInfos = editInfo.ValueInfos.ToArray();

                    foreach (var valueInfo in editInfo.ValueInfos)
                    {
                        foreach (var inputLevelPair in inputLevelIds)
                        {
                            valueInfo.CoordinateFilter.Add(inputLevelPair.Key, new[] { inputLevelPair.Value });
                        }
                    }

                    result.Add(editInfo);
                }
            }

            return result;

            object GetCostElementValue(IXLCell cell, CostElementInfo costElementInfo)
            {
                object value;

                switch (costElementInfo.Type)
                {
                    case CostElementType.Numeric:
                        value = cell.GetDouble();
                        break;

                    case CostElementType.Reference:
                        var refCellValue = cell.GetString().Trim().ToUpper();

                        value = costElementInfo.ReferenceItems[refCellValue].Id;
                        break;

                    case CostElementType.Percent:
                        value = cell.GetDouble() * 100;
                        break;

                    default:
                        throw new Exception("CostElementValue error");
                }

                return value;
            }
        }

        private async Task<ExcelColumsInfo> BuildExcelColumsInfo(IXLWorksheet worksheet, CostBlockEntityMeta[] costBlocks) 
        {
            var excelColumsInfo = new ExcelColumsInfo();
            var dependencyItemsCache = new Dictionary<string, Dictionary<string, NamedId>>();
            var references = GetReferences();

            var costElements =
                costBlocks.SelectMany(costBlock => costBlock.CostElementsFields.Select(costElement => new
                          {
                              CostBlock = costBlock,
                              CostElement = costBlock.DomainMeta.CostElements[costElement.Name]
                          }))
                         .ToDictionary(info => info.CostElement.Name.Trim().ToUpper());

            costElements = this.PrapareByNameMapping(costElements);

            var columnCount = worksheet.ColumnsUsed().Count();

            for (var columnIndex = 1; columnIndex <= columnCount; columnIndex++)
            {
                var value = worksheet.Cell(1, columnIndex).GetString();

                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Trim().ToUpper();

                    var costElementInfo = await GetCostElementInfo(value);
                    if (costElementInfo == null)
                    {
                        excelColumsInfo.InputLevelInfos.Add(columnIndex, references[value]);
                    }
                    else
                    {
                        excelColumsInfo.CostElementsInfos.Add(columnIndex, costElementInfo);
                    }
                }
            }

            return excelColumsInfo;

            Dictionary<string, NamedEntityMeta> GetReferences()
            {
                var result = new Dictionary<string, NamedEntityMeta>();

                foreach (var costBlock in costBlocks)
                {
                    var coordinateInfos = costBlock.DomainMeta.Coordinates.Select(coordiante => new
                    {
                        Field = (ReferenceFieldMeta)costBlock.GetField(coordiante.Id),
                        Name = coordiante.Name.Trim().ToUpper()
                    });

                    var costElementInfos =
                        costBlock.CostElementsFields.OfType<ReferenceFieldMeta>()
                                                    .Select(field => new
                                                    {
                                                        Field = field,
                                                        Name = costBlock.DomainMeta.CostElements[field.Name].Name.Trim().ToUpper()
                                                    });

                    foreach(var info in coordinateInfos.Concat(costElementInfos))
                    {
                        if (result.TryGetValue(info.Name, out var namedIdMeta))
                        {
                            if (namedIdMeta != info.Field.ReferenceMeta)
                            {
                                throw new Exception("Reference metas dublication");
                            }
                        }
                        else
                        {
                            result[info.Name] = (NamedEntityMeta)info.Field.ReferenceMeta;
                        }
                    }
                }

                return this.PrapareByNameMapping(result);
            }

            async Task<CostElementInfo> GetCostElementInfo(string value)
            {
                const string Separator = " ";

                CostElementInfo result = null;

                var splitedValue = value.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
                var lastIndex = splitedValue.Length - 1;

                for (var index = lastIndex; index >= 0; index--)
                {
                    var costElementName = string.Join(Separator, splitedValue.Take(index + 1)).Trim().ToUpper();

                    if (costElements.TryGetValue(costElementName, out var info))
                    {
                        result = new CostElementInfo
                        {
                            CostBlockMeta = info.CostBlock,
                            CostElement = info.CostElement,
                        };

                        switch(info.CostElement.GetOptionsType())
                        {
                            case "Reference":
                                var schema = info.CostElement.TypeOptions["Schema"];
                                var table = info.CostElement.TypeOptions["Name"];
                                var refMeta = (NamedEntityMeta)this.meta.GetEntityMeta(table, schema);

                                result.ReferenceItems = await this.GetNamedIds(refMeta);
                                result.ReferenceItems = this.PrapareByNameMapping(result.ReferenceItems);
                                result.Type = CostElementType.Reference;
                                break;

                            case "Percent":
                                result.Type = CostElementType.Percent;
                                break;

                            default:
                                result.Type = CostElementType.Numeric;
                                break;
                        }

                        if (index != lastIndex && info.CostElement.Dependency != null)
                        {
                            var dependencyMetaName = info.CostElement.Dependency.Name.Trim().ToUpper();
                            var dependencyItemName = string.Join(Separator, splitedValue.Skip(index + 1)).Trim().ToUpper();
                            var dependencyItem = await this.GetNamedId(references[dependencyMetaName], dependencyItemName, dependencyItemsCache);

                            result.DependencyItemId = dependencyItem.Id;
                        }
                    }
                }

                return result;
            }
        }

        private async Task<Dictionary<string, NamedId>> GetNamedIds(NamedEntityMeta meta)
        {
            var namedIds = await sqlRepository.GetNameIdItems(meta, meta.IdField.Name, meta.NameField.Name);

            return
                namedIds.ToDictionary(dependencyItem => dependencyItem.Name.Trim().ToUpper());

        }

        private async Task<NamedId> GetNamedId(NamedEntityMeta meta, string itemName, Dictionary<string, Dictionary<string, NamedId>> cache)
        {
            if (!cache.TryGetValue(meta.FullName, out var items))
            {
                items = await this.GetNamedIds(meta);
                items = this.PrapareByNameMapping(items);

                cache.Add(meta.FullName, items);
            }

            itemName = itemName.Trim().ToUpper();

            return items[itemName];
        }

        private Dictionary<string, T> PrapareByNameMapping<T>(Dictionary<string, T> dictionary)
        {
            var resultDictionary = new Dictionary<string, T>(dictionary);

            foreach (var keyValue in this.nameMapping)
            {
                if (dictionary.TryGetValue(keyValue.Value, out var value))
                {
                    resultDictionary[keyValue.Key] = value;
                }
            }

            return resultDictionary;
        }

        private class ExcelColumsInfo
        {
            public Dictionary<int, NamedEntityMeta> InputLevelInfos { get; } = new Dictionary<int, NamedEntityMeta>();

            public Dictionary<int, CostElementInfo> CostElementsInfos { get; } = new Dictionary<int, CostElementInfo>();
        }

        private enum CostElementType
        {
            Numeric,
            Percent,
            Reference,
        }

        private class CostElementInfo
        {
            public CostBlockEntityMeta CostBlockMeta { get; set; }

            public CostElementMeta CostElement { get; set; }

            public long? DependencyItemId { get; set; }

            public Dictionary<string, NamedId> ReferenceItems { get; set; }

            public CostElementType Type { get; set; }
        }
    }
}
