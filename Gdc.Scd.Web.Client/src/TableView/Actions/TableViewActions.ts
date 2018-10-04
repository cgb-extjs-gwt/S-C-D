import { CommonAction } from "../../Common/Actions/CommonActions";
import { TableViewInfo } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";
import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { updateRecords } from "../Services/TableViewService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";

export const TABLE_VIEW_LOAD_INFO = 'TABLE_VIEW.LOAD.INFO'
export const TABLE_VIEW_EDIT_RECORD = 'TABLE_VIEW.EDIT.RECORD'
export const TABLE_VIEW_RESET_CHANGES = 'TABLE_VIEW.RESET.CHANGES'

export interface EditRecordAction extends Action<string> {
    record: TableViewRecord
    dataIndex: string
}

export const loadTableViewInfo = (tableViewInfo: TableViewInfo) => (<CommonAction<TableViewInfo>>{
    type: TABLE_VIEW_LOAD_INFO,
    data: tableViewInfo
})

export const editRecord = (record: TableViewRecord, dataIndex: string) => (<EditRecordAction>{
    type: TABLE_VIEW_EDIT_RECORD,
    record,
    dataIndex
})

export const resetChanges = () => (<Action<string>>{
    type: TABLE_VIEW_RESET_CHANGES
})

// export const saveChanges = () => asyncAction<CommonState>(
//     (dispatch, getState) => {
//         const state = getState();

//         handleRequest(
//             updateRecords(state.pages.tableView.editedRecords).then(
//                 () => dispatch(resetChanges())
//             )
//         );
//     }
// )