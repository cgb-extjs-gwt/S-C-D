import { Reducer, Action } from "redux";
import { TABLE_VIEW_LOAD_INFO, TABLE_VIEW_EDIT_RECORD, EditRecordAction, TABLE_VIEW_RESET_CHANGES } from "../Actions/TableViewActions";
import { TableViewState, TableViewInfo } from "../States/TableViewState";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { TableViewRecord } from "../States/TableViewRecord";
import { isEqualCoordinates } from "../Helpers/TableViewHelper";

const init = () => (<TableViewState>{
    info: null,
    editedRecords: []
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
                    additionalData: actionRecord.additionalData
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
                            additionalData: actionRecord.additionalData
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

export const tableViewReducer: Reducer<TableViewState, Action<string>> = (state = init(), action) => {
    switch(action.type) {
        case TABLE_VIEW_LOAD_INFO:
            return loadInfo(state, action as CommonAction<TableViewInfo>);

        case TABLE_VIEW_EDIT_RECORD:
            return editRecord(state, action as EditRecordAction);

        case TABLE_VIEW_RESET_CHANGES:
            return resetChanges(state, action);

        default:
            return state;
    }
}