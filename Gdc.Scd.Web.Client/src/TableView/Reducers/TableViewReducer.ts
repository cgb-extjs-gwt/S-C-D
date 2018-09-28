import { Reducer, Action } from "redux";
import { TABLE_VIEW_LOAD_INFO } from "../Actions/TableViewActions";
import { TableViewState, TableViewInfo } from "../States/TableViewState";
import { CommonAction } from "../../Common/Actions/CommonActions";

const init = () => (<TableViewState>{
    info: null
})

const loadInfo: Reducer<TableViewState, CommonAction<TableViewInfo>> = (state, action) => ({
    info: action.data
})

export const tableViewReducer: Reducer<TableViewState, Action<string>> = (state = init(), action) => {
    switch(action.type) {
        case TABLE_VIEW_LOAD_INFO:
            return loadInfo(state, action as CommonAction<TableViewInfo>);

        default:
            return state;
    }
}