import { CommonAction } from "../../Common/Actions/CommonActions";
import { TableViewInfo, QualityGateResultSet, ImportResult } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";
import { Action } from "redux";

export const TABLE_VIEW_LOAD_INFO = 'TABLE_VIEW.LOAD.INFO'
export const TABLE_VIEW_EDIT_RECORD = 'TABLE_VIEW.EDIT.RECORD'
export const TABLE_VIEW_RESET_CHANGES = 'TABLE_VIEW.RESET.CHANGES'
export const TABLE_VIEW_LOAD_QUALITY_CHECK_RESULT = 'TABLE_VIEW.LOAD.QUALITY_CHECK_RESULT'
export const TABLE_VIEW_RESET_QUALITY_CHECK_RESULT = 'TABLE_VIEW.RESET.QUALITY_CHECK_RESULT'
export const TABLE_VIEW_LOAD_IMPORT_RESULTS = 'TABLE_VIEW.LOAD.IMPORT_RESULTS'
export const TABLE_VIEW_RESET_IMPORT_RESULTS = 'TABLE_VIEW.RESET.IMPORT_RESULTS'
export const TABLE_VIEW_LOAD_FILE_DATA = 'TABLE_VIEW.LOAD.FILE_DATA'

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

export const loadImportResults = (importResults: string[]) => (<CommonAction<ImportResult[]>>{
    type: TABLE_VIEW_LOAD_IMPORT_RESULTS,
    data: importResults.map(status => <ImportResult>{ status })
})

export const resetImportResults = () => (<Action<string>>{
    type: TABLE_VIEW_RESET_IMPORT_RESULTS
})

export const loadFileData = (base64Data: string) => (<CommonAction<string>>{
    type: TABLE_VIEW_LOAD_FILE_DATA,
    data: base64Data
})