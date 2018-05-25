import { Action } from "redux";

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

const defaultState = () => (<CostElementState>{
    title: 'Cost Elements',
    isLoading: true,
});

export const costElementReducer = (state: CostElementState = defaultState(), action: Action<string>) => {
    switch(action.type) {
        default:
            return state;
    }
}

