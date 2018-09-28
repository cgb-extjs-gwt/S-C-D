import { CommonAction } from "../../Common/Actions/CommonActions";
import { TableViewInfo } from "../States/TableViewState";

export const TABLE_VIEW_LOAD_INFO = 'TABLE_VIEW.LOAD.INFO'

export const loadTableViewInfo = (tableViewInfo: TableViewInfo) => (<CommonAction<TableViewInfo>>{
    type: TABLE_VIEW_LOAD_INFO,
    data: tableViewInfo
})