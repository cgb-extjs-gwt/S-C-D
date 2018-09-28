import { buildMvcUrl, get } from "../../Common/Services/Ajax";
import { TableViewInfo } from "../States/TableViewState";

const TABLE_VIEW_CONTROLLER_NAME = 'TableView';

export const buildGetRecordsUrl = () => buildMvcUrl(TABLE_VIEW_CONTROLLER_NAME, 'GetRecords')

export const getTableViewInfo = () => get<TableViewInfo>(TABLE_VIEW_CONTROLLER_NAME, 'GetTableViewInfo')