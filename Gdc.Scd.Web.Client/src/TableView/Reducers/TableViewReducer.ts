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
    const recordIndex = state.editedRecords.findIndex(editRecord => isEqualCoordinates(editRecord,  action.record));
        
    let editedRecords: TableViewRecord[];

    const changedData = { 
        [action.dataIndex]: action.record.data[action.dataIndex]
    };

    if (recordIndex == -1) {
        editedRecords = [
            ...state.editedRecords, 
            {
                coordinates: action.record.coordinates,
                data: changedData
            }
        ];
    }
    else {
        editedRecords = state.editedRecords.map(
            (record, index) => 
                index == recordIndex 
                    ? {
                        coordinates: action.record.coordinates,
                        data: { 
                            ...record.data, 
                            ...changedData
                        }
                    }
                    : record
        );
    }

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