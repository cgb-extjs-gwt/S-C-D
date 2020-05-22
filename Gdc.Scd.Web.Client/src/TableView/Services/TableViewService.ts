import { buildMvcUrl, get, post, downloadFile } from "../../Common/Services/Ajax";
import { TableViewInfo, QualityGateResultSet } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";
import { ImportData } from "../../Common/States/ImportData";
import { ImportResult } from "../States/ImportResult";

const TABLE_VIEW_CONTROLLER_NAME = 'TableView';

export const buildGetRecordsUrl = () => buildMvcUrl(TABLE_VIEW_CONTROLLER_NAME, 'GetRecords')

export const getTableViewInfo = () => get<TableViewInfo>(TABLE_VIEW_CONTROLLER_NAME, 'GetTableViewInfo')

export const updateRecords = (records: TableViewRecord[], approvalOption: ApprovalOption) => 
    post<TableViewRecord[], QualityGateResultSet>(TABLE_VIEW_CONTROLLER_NAME, 'UpdateRecords', records, approvalOption)

export const buildGetHistoryUrl = (costElementId: CostElementIdentifier, coordinates: { [key: string]: number }) => 
    buildMvcUrl(TABLE_VIEW_CONTROLLER_NAME, 'GetHistory', { costElementId, coordinates: JSON.stringify(coordinates) });

export const exportToExcel = () => downloadFile(TABLE_VIEW_CONTROLLER_NAME, 'ExportToExcel');

export const importFromExcel = (importData: ImportData) => post<any, ImportResult>(TABLE_VIEW_CONTROLLER_NAME, 'ImportExcel', importData)