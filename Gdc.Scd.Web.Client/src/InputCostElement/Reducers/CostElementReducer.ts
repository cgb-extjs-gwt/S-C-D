import { Action, Reducer } from "redux";
import { PageState } from "../../Layout/States/PageStates";
import { CommonAction } from "../../Common/CommonAction";
import { CostElementInput } from "../States/CostElementState";
import { PageAction, PAGE_INIT_SUCCESS } from "../../Layout/Actions/PageActions";
import { 
    COST_ELEMENT_INTPUT_PAGE, 
    COST_ELEMENT_INTPUT_SELECT_APPLICATION, 
    COST_ELEMENT_INTPUT_SELECT_SCOPE,
    ItemSelectedAction
} from "../Actions/InputCostElementActions";
import { SelectList } from "../../Common/States/SelectList";
import { NamedId } from "../../Common/States/NamedId";

const selectFirstItem = <T extends NamedId>(selectList: SelectList<NamedId>) => (<SelectList<T>>{
    ...selectList,
    selectedItemId: selectList.list[0].id
});

const selectItem = <T>(selectList: SelectList<T>, action: Action<string>) => (<SelectList<T>>{
    ...selectList,
    selectedItemId: (<ItemSelectedAction>action).selectedItemId
})

export const costElementReducer: Reducer<CostElementInput, Action<string>> = (state = <CostElementInput>{}, action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return (<PageAction>action).pageName === COST_ELEMENT_INTPUT_PAGE 
                ? {
                    ...state,
                    applications: selectFirstItem(state.applications),
                    scopes: selectFirstItem(state.scopes),
                    costBlocks: selectFirstItem(state.costBlocks)
                } 
                : state;

        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
            return {
                ...state,
                applications: selectItem(state.applications, action),
                costBlocks: {
                    ...state.costBlocks,
                    list: state.costBlocks.
                }
            }

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
            return {
                ...state,
                scopes: selectItem(state.scopes, action)
            }

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
            return {
                ...state,
                costBlocks: selectItem(state.costBlocks, action)
            }

        default:
            return state;
    }
}

