import { buildMvcUrl, get, post } from "../../Common/Services/Ajax";
import { TableViewInfo } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";

const TABLE_VIEW_CONTROLLER_NAME = 'TableView';

export const buildGetRecordsUrl = () => buildMvcUrl(TABLE_VIEW_CONTROLLER_NAME, 'GetRecords')

export const getTableViewInfo = () => get<TableViewInfo>(TABLE_VIEW_CONTROLLER_NAME, 'GetTableViewInfo')

export const updateRecords = (records: TableViewRecord[]) => post<TableViewRecord[]>(TABLE_VIEW_CONTROLLER_NAME, 'UpdateRecords', records)