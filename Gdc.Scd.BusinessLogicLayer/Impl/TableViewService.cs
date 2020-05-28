using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.QualityGate;
using Gdc.Scd.Core.Entities.TableView;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class TableViewService : ITableViewService
    {
        private readonly ITableViewRepository tableViewRepository;

        private readonly IUserService userService;

        private readonly ICostBlockHistoryService costBlockHistoryService;

        private readonly IDomainService<Wg> wgService;

        private readonly IRoleCodeService roleCodeService;

        private readonly ICostBlockService costBlockService;

        private readonly ISqlRepository sqlRepository;

        private readonly IExcelConverterService converterService;

        private readonly DomainEnitiesMeta meta;

        private readonly DomainMeta domainMeta;

        public TableViewService(
            ITableViewRepository tableViewRepository, 
            IUserService userService, 
            ICostBlockHistoryService costBlockHistoryService,
            IDomainService<Wg> wgService,
            IRoleCodeService roleCodeService,
            ICostBlockService costBlockService,
            ISqlRepository sqlRepository,
            IExcelConverterService converterService,
            DomainEnitiesMeta meta,
            DomainMeta domainMeta)
        {
            this.tableViewRepository = tableViewRepository;
            this.userService = userService;
            this.costBlockHistoryService = costBlockHistoryService;
            this.wgService = wgService;
            this.roleCodeService = roleCodeService;
            this.costBlockService = costBlockService;
            this.sqlRepository = sqlRepository;
            this.converterService = converterService;
            this.meta = meta;
            this.domainMeta = domainMeta;
        }

        public async Task<IEnumerable<Record>> GetRecords()
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var records = await this.tableViewRepository.GetRecords(costBlockInfos);
            var recordDictionary = this.GetRecordDictionary(records);
            var wgs = this.GetWgs(recordDictionary.Keys);

            foreach (var wg in wgs)
            {
                var record = recordDictionary[wg.Id];

                record.WgRoleCodeId = wg.RoleCodeId;
                record.WgResponsiblePerson = wg.ResponsiblePerson;
                record.WgPsmRelease = wg.PsmRelease;
            }

            return records;
        }

        public async Task<QualityGateResultSet> UpdateRecords(IEnumerable<Record> records, ApprovalOption approvalOption)
        {
            QualityGateResultSet qualityGateResult;

            var costBlockInfos = this.GetCostBlockInfo().ToArray();
            var editInfos = this.tableViewRepository.BuildEditInfos(costBlockInfos, records).ToArray();

            qualityGateResult = editInfos.Length == 0
                ? new QualityGateResultSet()
                : await this.costBlockService.Update(editInfos, approvalOption, EditorType.TableView);

            var recordDictionary = this.GetRecordDictionary(records);
            var wgs = this.GetWgs(recordDictionary.Keys);
            var updatedWgs = new List<Wg>();

            foreach (var wg in wgs)
            {
                var record = recordDictionary[wg.Id];

                if (wg.RoleCodeId != record.WgRoleCodeId ||
                    wg.ResponsiblePerson != record.WgResponsiblePerson ||
                    wg.PsmRelease != record.WgPsmRelease)
                {
                    wg.RoleCodeId = record.WgRoleCodeId;
                    wg.ResponsiblePerson = record.WgResponsiblePerson;
                    wg.PsmRelease = record.WgPsmRelease;

                    updatedWgs.Add(wg);
                }
            }

            this.wgService.SaveWithoutTransaction(updatedWgs);

            return qualityGateResult;
        }

        public async Task<TableViewInfo> GetTableViewInfo()
        {
            var costBlockInfos = this.GetCostBlockInfo().ToArray();

            return new TableViewInfo
            {
                RecordInfo = await this.tableViewRepository.GetRecordInfo(costBlockInfos),
                CostBlockReferences = await this.tableViewRepository.GetReferences(costBlockInfos),
                DependencyItems = await this.tableViewRepository.GetDependencyItems(costBlockInfos),
                RoleCodeReferences = 
                    (await this.roleCodeService.GetAllActive())
                                        .Select(roleCode => new NamedId { Id = roleCode.Id, Name = roleCode.Name })
                                        .ToArray()
            };
        }

        public async Task<DataInfo<HistoryItemDto>> GetHistory(CostElementIdentifier costElementId, IDictionary<string, long> coordinates, QueryInfo queryInfo = null)
        {
            var historyContext = new CostElementContext
            {
                ApplicationId = costElementId.ApplicationId,
                CostBlockId = costElementId.CostBlockId,
                CostElementId = costElementId.CostElementId
            };

            var costBlockMeta = this.meta.CostBlocks[historyContext];
            var inputLevel = costBlockMeta.SliceDomainMeta.GetMaxInputLevel(coordinates.Keys);

            historyContext.InputLevelId = inputLevel.Id;

            var filter = coordinates.ToDictionary(keyValue => keyValue.Key, keyValue => new[] { keyValue.Value });

            return await this.costBlockHistoryService.GetHistory(historyContext, filter, queryInfo);
        }

        public async Task<Stream> ExportToExcel()
        {
            const int StartRow = 1;
            const int StartColumn = 1;

            var records = await this.GetRecords();
            var tableViewInfo = await this.GetTableViewInfo();
            var costElementInfos =
                    tableViewInfo.RecordInfo.Data.GroupBy(dataInfo => new
                                                 {
                                                     dataInfo.ApplicationId,
                                                     dataInfo.CostBlockId,
                                                     CostElement = this.domainMeta.GetCostElement(dataInfo)
                                                 })
                                                 .Select(group => new
                                                 {
                                                     group.Key.CostElement,
                                                     DataInfos = group.ToArray()
                                                 })
                                                 .ToArray();

            MemoryStream stream;

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Central Data Input");
                var headerInfo = WriteHeaders(sheet, StartRow, StartColumn);
                var dataInfo = WriteData(sheet, headerInfo.EndRow + 1, StartColumn);

                for (var column = StartColumn; column <= headerInfo.EndColumn; column++)
                {
                    sheet.Column(column).AdjustToContents();
                }

                sheet.Range(headerInfo.EndRow, StartColumn, dataInfo.EndRow, dataInfo.EndColumn).SetAutoFilter();

                workbook.SaveAs(stream = new MemoryStream());

                stream.Position = 0;
            }

            return stream;

            (int EndRow, int EndColumn) WriteHeaders(IXLWorksheet sheet, int row, int column)
            {
                var endRow =
                    tableViewInfo.RecordInfo.Data.Any(dataInfo => dataInfo.DependencyItemId.HasValue)
                        ? row + 1
                        : row;

                foreach (var coordinateId in tableViewInfo.RecordInfo.Coordinates)
                {
                    var coordinateMeta = this.domainMeta.GetCoordinate(coordinateId);

                    sheet.Range(row, column, endRow, column++).Merge().Value = coordinateMeta.Caption;
                }

                foreach (var additionalData in tableViewInfo.RecordInfo.AdditionalData)
                {
                    sheet.Range(row, column, endRow, column++).Merge().Value = additionalData.Title;
                }

                sheet.Range(row, column, endRow, column++).Merge().Value = "Role code";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Responsible person";
                sheet.Range(row, column, endRow, column++).Merge().Value = "PSM Release";

                foreach (var info in costElementInfos)
                {
                    var dependency = info.CostElement.Dependency;

                    if (dependency == null)
                    {
                        sheet.Range(row, column, endRow, column++).Merge().Value = info.CostElement.Caption;
                    }
                    else
                    {
                        var endColumn = column + info.DataInfos.Length - 1;

                        sheet.Range(row, column, row, endColumn).Merge().Value = info.CostElement.Caption;

                        foreach (var dataInfo in info.DataInfos)
                        {
                            var item = tableViewInfo.GetDependencyItem(dependency.Id, dataInfo.DependencyItemId.Value);

                            sheet.Range(endRow, column, endRow, column++).Value = item.Name;
                        }
                    }
                }

                column--;

                var range = sheet.Range(row, 1, endRow, column);

                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Alignment.WrapText = true;
                range.Style.Font.Bold = true;

                return (endRow, column);
            }

            (int EndRow, int EndColumn) WriteData(IXLWorksheet sheet, int row, int startColumn)
            {
                var column = startColumn;

                foreach (var record in records)
                {
                    column = startColumn;

                    foreach (var coordinateId in tableViewInfo.RecordInfo.Coordinates)
                    {
                        sheet.Cell(row, column++).Value = record.Coordinates[coordinateId].Name;
                    }

                    foreach (var additionalData in record.AdditionalData)
                    {
                        sheet.Cell(row, column++).Value = additionalData.Value;
                    }

                    sheet.Cell(row, column++).Value = tableViewInfo.GetRoleCode(record)?.Name;
                    sheet.Cell(row, column++).Value = record.WgResponsiblePerson;
                    sheet.Cell(row, column++).Value = record.WgPsmRelease;

                    foreach (var dataInfo in costElementInfos.SelectMany(info => info.DataInfos))
                    {
                        var cell = sheet.Cell(row, column++);
                        var data = record.Data[dataInfo.DataIndex];

                        if (data.Count > 1)
                        {
                            cell.Value = $"({data.Count} values)";
                        }
                        else if (this.meta.CostBlocks[dataInfo].CostElementsFields[dataInfo.CostElementId] is ReferenceFieldMeta)
                        {
                            var referenceItems = tableViewInfo.CostBlockReferences[dataInfo.CostBlockId].References[dataInfo.CostElementId];
                            var referenceItem = referenceItems.FirstOrDefault(item => item.Id.Equals(data.Value));

                            if (referenceItem != null)
                            {
                                cell.Value = referenceItem.Name;
                            }
                        }
                        else
                        {
                            cell.Value = data.Value;
                        }

                        if (data.IsApproved)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                        }
                    }

                    row++;
                }

                return (row - 1, column - 1);
            }
        }

        public async Task<TableViewExcelImportResult> ImportFromExcel(Stream excelStream, ApprovalOption approvalOption)
        {
            const int StartRow = 3;
            const int StartColumn = 1;

            var result = new TableViewExcelImportResult
            {
                Errors = new List<string>()
            };

            using (var workbook = new XLWorkbook(excelStream))
            {
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
                    try
                    {
                        var column = StartColumn;
                        var tableViewInfo = await this.GetTableViewInfo();

                        var coordinateHandlers = new List<Func<IXLRow, Record, bool>>();

                        foreach (var coordinateId in tableViewInfo.RecordInfo.Coordinates)
                        {
                            coordinateHandlers.Add(await BuildCoordinateRecordHandler(column++, coordinateId));
                        }

                        column += tableViewInfo.RecordInfo.AdditionalData.Length;

                        var costElementHandlers = new List<Action<IXLRow, Record>> 
                        { 
                            await BuildRoleCodeRecordHandler(column++),
                            BuildResponsiblePersonRecordHandler(column++),
                            BuildPsmReleaseRecordHandler(column++)
                        };

                        foreach (var dataInfo in tableViewInfo.RecordInfo.Data)
                        {
                            costElementHandlers.Add(await BuildCostElementRecordHandler(column++, dataInfo));
                        }

                        var newRecords = new List<Record>();

                        foreach (var row in worksheetInfo.RowsUsed.Skip(StartRow - 1))
                        {
                            try
                            {
                                var record = new Record();

                                if (coordinateHandlers.All(handler => handler(row, record)))
                                {
                                    foreach (var costElementHandler in costElementHandlers)
                                    {
                                        try
                                        {
                                            costElementHandler(row, record);
                                        }
                                        catch (Exception ex)
                                        {
                                            result.Errors.Add($"Row number: {row.RowNumber()}. {ex.Message}");
                                        }
                                    }

                                    newRecords.Add(record);
                                }
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Row number: {row.RowNumber()}. {ex.Message}");
                            }
                        }

                        var oldRecords = await this.GetRecords();
                        var records = GetUpdateRecords(oldRecords, newRecords).ToArray();

                        result.QualityGateResult = await this.UpdateRecords(records, approvalOption);
                    }
                    catch(Exception ex)
                    {
                        result.Errors.Add(ex.Message);
                    }
                }
            }

            return result;

            async Task<Func<IXLRow, Record, bool>> BuildCoordinateRecordHandler(int column, string coordinateId)
            {
                var coordMeta = this.meta.GetInputLevel(coordinateId);
                var converter = await this.converterService.BuildReferenceConverter(coordMeta);

                return (row, record) =>
                {
                    var isContinueHandle = true;

                    var value = row.Cell(column).GetValue<string>();

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        isContinueHandle = false;
                    }
                    else
                    {
                        record.Coordinates.Add(coordinateId, converter(value));
                    }

                    return isContinueHandle;
                };
            }

            async Task<Action<IXLRow, Record>> BuildCostElementRecordHandler(int column, DataInfo dataInfo)
            {
                var costBlock = this.meta.CostBlocks[dataInfo];
                var converterFn = await this.converterService.BuildConverter(costBlock, dataInfo.CostElementId);

                return (row, record) =>
                {
                    var value = row.Cell(column).GetValue<string>();
                    var convertedValue = converterFn(value);

                    record.Data[dataInfo.DataIndex] = new TableViewCellData
                    {
                        Value = converterFn(value)
                    };
                };
            }

            async Task<Action<IXLRow, Record>> BuildRoleCodeRecordHandler(int column)
            {
                var roleCodeMeta = (NamedEntityMeta)this.meta.GetEntityMeta<RoleCode>();
                var converter = await this.converterService.BuildReferenceConverter(roleCodeMeta);

                return (row, record) =>
                {
                    var value = row.Cell(column).GetValue<string>();
                    var roleCode = converter(value);

                    record.WgRoleCodeId = roleCode?.Id;
                };
            }

            Action<IXLRow, Record> BuildResponsiblePersonRecordHandler(int column)
            {
                return (row, record) =>
                {
                    record.WgResponsiblePerson = row.Cell(column).GetValue<string>();
                };
            }

            Action<IXLRow, Record> BuildPsmReleaseRecordHandler(int column)
            {
                return (row, record) =>
                {
                    var value = row.Cell(column).GetValue<string>();

                    record.WgPsmRelease = this.converterService.ConvertToBool(value);
                };
            }

            IEnumerable<Record> GetUpdateRecords(IEnumerable<Record> oldRecords, IEnumerable<Record> newRecords)
            {
                var joinedRecords =
                    oldRecords.Join(newRecords, GetJoinKey, GetJoinKey, (oldRec, newRec) => (oldRec, newRec));

                foreach (var (oldRecord, newRecord) in joinedRecords)
                {
                    var updatedData = new Dictionary<string, TableViewCellData>();

                    foreach (var oldData in oldRecord.Data)
                    {
                        if (newRecord.Data.TryGetValue(oldData.Key, out var newData) && 
                            !Equals(oldData.Value.Value, newData.Value))
                        {
                            updatedData.Add(oldData.Key, newData);
                        }
                    }

                    if (updatedData.Count > 0 ||
                        !Equals(oldRecord.WgPsmRelease, newRecord.WgPsmRelease) ||
                        !Equals(oldRecord.WgResponsiblePerson, newRecord.WgResponsiblePerson) ||
                        !Equals(oldRecord.WgRoleCodeId, newRecord.WgRoleCodeId))
                    {
                        yield return new Record
                        {
                            Coordinates = oldRecord.Coordinates,
                            Data = updatedData,
                            WgPsmRelease = newRecord.WgPsmRelease,
                            WgResponsiblePerson = newRecord.WgResponsiblePerson,
                            WgRoleCodeId = newRecord.WgRoleCodeId
                        };
                    }
                }

                string GetJoinKey(Record record)
                {
                    var items =
                        record.Coordinates.OrderBy(keyValue => keyValue.Key)
                                          .Select(keyValue => $"{keyValue.Key}:{keyValue.Value.Id}");

                    return string.Join(",", items);
                }
            }
        }

        private IDictionary<long, Record> GetRecordDictionary(IEnumerable<Record> records)
        {
            return records.ToDictionary(record => record.Coordinates[MetaConstants.WgInputLevelName].Id);
        }

        private Wg[] GetWgs(IEnumerable<long> wgIds)
        {
            return this.wgService.GetAll().Where(wg => wgIds.Contains(wg.Id)).ToArray();
        }

        private IEnumerable<CostElementInfo> GetCostBlockInfo()
        {
            var user = this.userService.GetCurrentUser();

            foreach (var costBlock in this.meta.CostBlocks)
            {
                var fieldNames =
                    (from costElement in costBlock.SliceDomainMeta.CostElements
                     where costElement.TableViewRoles != null && user.Roles.Any(role => costElement.TableViewRoles.Contains(role.Name))
                     select costElement.Id).ToArray();

                if (fieldNames.Length > 0)
                {
                    yield return new CostElementInfo
                    {
                        Meta = costBlock,
                        CostElementIds = fieldNames
                    };
                }
            }
        }
    }
}
