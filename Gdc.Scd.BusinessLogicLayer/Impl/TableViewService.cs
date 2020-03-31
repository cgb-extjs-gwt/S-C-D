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

        private readonly DomainEnitiesMeta meta;

        private readonly DomainMeta domainMeta;

        public TableViewService(
            ITableViewRepository tableViewRepository, 
            IUserService userService, 
            ICostBlockHistoryService costBlockHistoryService,
            IDomainService<Wg> wgService,
            IRoleCodeService roleCodeService,
            ICostBlockService costBlockService,
            DomainEnitiesMeta meta,
            DomainMeta domainMeta)
        {
            this.tableViewRepository = tableViewRepository;
            this.userService = userService;
            this.costBlockHistoryService = costBlockHistoryService;
            this.wgService = wgService;
            this.roleCodeService = roleCodeService;
            this.costBlockService = costBlockService;
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

            var costBlockMeta = this.meta.GetCostBlockEntityMeta(historyContext);
            var inputLevel = costBlockMeta.DomainMeta.GetMaxInputLevel(coordinates.Keys);

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

                    sheet.Range(row, column, endRow, column++).Merge().Value = coordinateMeta.Name;
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
                        sheet.Range(row, column, endRow, column++).Merge().Value = info.CostElement.Name;
                    }
                    else
                    {
                        var endColumn = column + info.DataInfos.Length - 1;

                        sheet.Range(row, column, row, endColumn).Merge().Value = info.CostElement.Name;

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

                        cell.Value = data.Count > 1 ? $"({data.Count} values)" : data.Value;

                        if (data.IsApproved)
                        {
                            cell.Style.Fill.BackgroundColor = XLColor.Green;
                        }
                    }

                    row++;
                }

                return (row - 1, column - 1);
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
                    (from costElement in costBlock.DomainMeta.CostElements
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

        private (string Id, long Value)? GetMaxInputLevel(
            IDictionary<string, long> coordinates,
            IDictionary<string, InputLevelMeta> inputLevelMetas)
        {
            InputLevelMeta maxInputLevelMeta = null;
            (string, long)? maxInputLevel = null;

            foreach (var coordinate in coordinates)
            {
                if (inputLevelMetas.TryGetValue(coordinate.Key, out var inputLevelMeta) &&
                    (maxInputLevelMeta == null || maxInputLevelMeta.LevelNumber < inputLevelMeta.LevelNumber))
                {
                    maxInputLevelMeta = inputLevelMeta;
                    maxInputLevel = (coordinate.Key, coordinate.Value);
                }
            }

            return maxInputLevel;
        }
    }
}
