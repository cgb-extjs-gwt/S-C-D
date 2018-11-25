import { CommonAction } from "../../Common/Actions/CommonActions";
import { TableViewInfo, QualityGateResultSet } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";
import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { updateRecords } from "../Services/TableViewService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";

export const TABLE_VIEW_LOAD_INFO = 'TABLE_VIEW.LOAD.INFO'
export const TABLE_VIEW_EDIT_RECORD = 'TABLE_VIEW.EDIT.RECORD'
export const TABLE_VIEW_RESET_CHANGES = 'TABLE_VIEW.RESET.CHANGES'
export const TABLE_VIEW_LOAD_QUALITY_CHECK_RESULT = 'TABLE_VIEW.LOAD.QUALITY_CHECK_RESULT'
export const TABLE_VIEW_RESET_QUALITY_CHECK_RESULT = 'TABLE_VIEW.RESET.QUALITY_CHECK_RESULT'

export interface EditRecordAction extends Action<string> {
    records: TableViewRecord[]
    dataIndex: string
}

export const loadTableViewInfo = (tableViewInfo: TableViewInfo) => (<CommonAction<TableViewInfo>>{
    type: TABLE_VIEW_LOAD_INFO,
    data: tableViewInfo
})

export const editRecord = (records: TableViewRecord[], dataIndex: string) => (<EditRecordAction>{
    type: TABLE_VIEW_EDIT_RECORD,
    records,
    dataIndex
})

export const resetChanges = () => (<Action<string>>{
    type: TABLE_VIEW_RESET_CHANGES
})

export const loadQualityCheckResult = (qualityGateResultSet: QualityGateResultSet) => (<CommonAction<QualityGateResultSet>>{
    type: TABLE_VIEW_LOAD_QUALITY_CHECK_RESULT,
    data: qualityGateResultSet
})

export const resetQualityCheckResult = () => (<Action<string>>{
    type: TABLE_VIEW_RESET_QUALITY_CHECK_RESULT
})

export const saveTableViewToServer = (approvalOption: ApprovalOption) => asyncAction<CommonState>(
    (dispatch, getState) => {
        const state = getState();

        handleRequest(
            updateRecords(state.pages.tableView.editedRecords, approvalOption).then(
                qualityGateResultSet => {
                    if (qualityGateResultSet.hasErros) {
                        dispatch(loadQualityCheckResult(qualityGateResultSet));
                    }
                    else {
                        dispatch(resetQualityCheckResult());
                        dispatch(resetChanges());
                    }
                }
            )
        )
    }
)
