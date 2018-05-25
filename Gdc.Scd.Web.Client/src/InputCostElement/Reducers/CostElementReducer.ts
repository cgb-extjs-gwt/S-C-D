import { Action, Reducer } from "redux";
import { PageState } from "../../Layout/States/PageStates";
import { CommonAction } from "../../Common/CommonAction";

export interface SelectList<T> {
    selected: T,
    list: T[]
}

export interface CostBlock {
    name: string
}

export interface CostElementState {
    title: string,
    isLoading: boolean,
    application: SelectList<string>,
    scope: SelectList<string>,
    costBlocks: SelectList<CostBlock>,
}

export const costElementReducer: Reducer<CostElementState, CommonAction> = (state = null, action) => {
    switch(action.type) {
        default:
            return state;
    }
}

