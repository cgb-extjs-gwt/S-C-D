import { Reducer, Action } from "redux";
import { TABLE_VIEW_LOAD_INFO, TABLE_VIEW_EDIT_RECORD, EditRecordAction, TABLE_VIEW_RESET_CHANGES, TABLE_VIEW_LOAD_QUALITY_CHECK_RESULT, TABLE_VIEW_RESET_QUALITY_CHECK_RESULT, TABLE_VIEW_LOAD_IMPORT_RESULTS, TABLE_VIEW_RESET_IMPORT_RESULTS, TABLE_VIEW_LOAD_FILE_DATA } from "../Actions/TableViewActions";
import { TableViewState, TableViewInfo, QualityGateResultSet, ImportResult } from "../States/TableViewState";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { TableViewRecord } from "../States/TableViewRecord";
import { isEqualCoordinates } from "../Helpers/TableViewHelper";

const init = () => (<TableViewState>{
    info: null,
    editedRecords: [],
    import: {
        fileBase64: null,
        results: []
    }
})

const loadInfo: Reducer<TableViewState, CommonAction<TableViewInfo>> = (state, action) => ({
    ...state,
    info: action.data
})

const editRecord: Reducer<TableViewState, EditRecordAction> = (state, action) => {
    let editedRecords = state.editedRecords;

    action.records.forEach(actionRecord => {
        const recordIndex = editedRecords.findIndex(editRecord => isEqualCoordinates(editRecord,  actionRecord));

        const changedData = { 
            [action.dataIndex]: actionRecord.data[action.dataIndex]
        };

        if (recordIndex == -1) {
            editedRecords = [
                ...editedRecords, 
                {
                    coordinates: actionRecord.coordinates,
                    data: changedData,
                    additionalData: actionRecord.additionalData,
                    wgRoleCodeId: actionRecord.wgRoleCodeId,
                    wgResponsiblePerson: actionRecord.wgResponsiblePerson,
                    wgPsmRelease: actionRecord.wgPsmRelease
                }
            ];
        }
        else {
            editedRecords = editedRecords.map(
                (record, index) => 
                    index == recordIndex 
                        ? {
                            coordinates: actionRecord.coordinates,
                            data: { 
                                ...record.data, 
                                ...changedData
                            },
                            additionalData: actionRecord.additionalData,
                            wgRoleCodeId: actionRecord.wgRoleCodeId,
                            wgResponsiblePerson: actionRecord.wgResponsiblePerson,
                            wgPsmRelease: actionRecord.wgPsmRelease
                        }
                        : record
            );
        }
    });
    
    return {
        ...state,
        editedRecords
    }
}

const resetChanges: Reducer<TableViewState> = state => ({
    ...state,
    editedRecords: []
})

const loadQualityCheckResult: Reducer<TableViewState, CommonAction<QualityGateResultSet>> = (state, action) => ({
    ...state,
    qualityGateResultSet: action.data
})

const resetQualityCheckResult: Reducer<TableViewState> = state => ({
    ...state,
    qualityGateResultSet: null
})

const loadImportResults: Reducer<TableViewState, CommonAction<ImportResult[]>> = (state, action) => ({
    ...state,
    import: {
        ...state.import,
        results: action.data
    }
})

const resetImportResults: Reducer<TableViewState, Action<string>> = state => ({
    ...state,
    import: {
        ...state.import,
        results: null
    }
})

const loadFileData: Reducer<TableViewState, CommonAction<string>> = (state, { data }) => ({
    ...state,
    import: {
        ...state.import,
        fileBase64: data
    }
})

export const tableViewReducer: Reducer<TableViewState, Action<string>> = (state = init(), action) => {
    switch(action.type) {
        case TABLE_VIEW_LOAD_INFO:
            return loadInfo(state, action as CommonAction<TableViewInfo>);

        case TABLE_VIEW_EDIT_RECORD:
            return editRecord(state, action as EditRecordAction);

        case TABLE_VIEW_RESET_CHANGES:
            return resetChanges(state, action);

        case TABLE_VIEW_LOAD_QUALITY_CHECK_RESULT:
            return loadQualityCheckResult(state, action as CommonAction<QualityGateResultSet>);

        case TABLE_VIEW_RESET_QUALITY_CHECK_RESULT:
            return resetQualityCheckResult(state, action);

        case TABLE_VIEW_LOAD_IMPORT_RESULTS:
            return loadImportResults(state, <CommonAction<ImportResult[]>>action);

        case TABLE_VIEW_RESET_IMPORT_RESULTS:
            return resetImportResults(state, action);

        case TABLE_VIEW_LOAD_FILE_DATA:
            return loadFileData(state, <CommonAction<string>>action);

        default:
            return state;
    }
}