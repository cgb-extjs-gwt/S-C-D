import { Action, Reducer } from "redux";
import { PageState } from "../../Layout/States/PageStates";
import { CommonAction } from "../../Common/CommonAction";
import { CostElementInputState, CostElementInputDto, CostBlockMeta, CostElementMeta } from "../States/CostElementState";
import { PageAction, PAGE_INIT_SUCCESS } from "../../Layout/Actions/PageActions";
import { 
    COST_ELEMENT_INTPUT_PAGE, 
    COST_ELEMENT_INTPUT_SELECT_APPLICATION, 
    COST_ELEMENT_INTPUT_SELECT_SCOPE,
    COST_ELEMENT_INTPUT_SELECT_COST_BLOCK,
    COST_ELEMENT_INTPUT_HIDE_LOSE_CHANGES_WARNING,
    COST_ELEMENT_INTPUT_LOSE_CHANGES,
    COST_ELEMENT_INTPUT_SHOW_LOSE_CHANGES_WARNING,
    ShowDataLoseWarningAction
} from "../Actions/InputCostElementActions";
import { SelectList } from "../../Common/States/SelectList";
import { NamedId } from "../../Common/States/NamedId";
import { CostBlockInputState, CostElementInput } from "../States/CostBlock";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import * as CostBlockReducers from "./CostBlockInputReducer"

const createMap = <T extends NamedId>(array: T[]) => {
    const map = new Map<string, T>();

    array.forEach(item => map.set(item.id, item));

    return map;
}

const initSuccess: Reducer<CostElementInputState, PageAction<CostElementInputDto>> = (state, action) => {
    const { applications, scopes, costBlockMetas, countries, inputLevels } = action.data;
    const selectedApplicationId = applications[0].id;
    const selectedScopeId = scopes[0].id;

    return action.pageName === COST_ELEMENT_INTPUT_PAGE 
        ? {
            ...state,
            applications: createMap(applications),
            scopes: createMap(scopes),
            countries: createMap(countries),
            costBlockMetas: createMap(costBlockMetas),
            inputLevels: createMap(inputLevels),
            selectedApplicationId,
            selectedScopeId,
        } 
        : state;
}

// const selectApplication = buildLoseDataChecker<ItemSelectedAction>(
//     (state, action) => ({
//         ...CostBlockReducers.selectApplication(state, action),
//         selectedApplicationId: action.selectedItemId
//     })
// )

// const selectScope = buildLoseDataChecker<ItemSelectedAction>(
//     (state, action) => ({
//         ...CostBlockReducers.selectScope(state, action),
//         selectedScopeId: (<ItemSelectedAction>action).selectedItemId
//     })
// )

const defaultState = () => (<CostElementInputState>{
    dataLossInfo: {
        isWarningDisplayed: false,
        action: null,
        //isLoseChanges: false
    }
})

const showDataLoseWarning: Reducer<CostElementInputState, ShowDataLoseWarningAction> = (state, action) => ({
    ...state,
    dataLossInfo: {
        ...state.dataLossInfo,
        isWarningDisplayed: true,
        action: action.dataLoseAction
    }
})

const hideDataLoseWarning: Reducer<CostElementInputState> = state => ({
    ...state,
    dataLossInfo: {
        ...state.dataLossInfo,
        isWarningDisplayed: false,
        action: null
    }
})

const loseChanges: Reducer<CostElementInputState, Action<string>> = state => ({
    ...state,
    costBlocksInputs: state.costBlocksInputs.map(costBlock => (<CostBlockInputState>{
        ...costBlock,
        edit: {
            ...costBlock.edit,
            editedItems: []
        }
    })),
    dataLossInfo: {
        ...state.dataLossInfo,
        //isLoseChanges: true
    }
})


export const costElementInputReducer: Reducer<CostElementInputState, Action<string>> = (state = defaultState(), action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return initSuccess(state, <PageAction<CostElementInputDto>>action);

        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
            //return selectApplication(state, action);
            return {
                ...state,
                selectedApplicationId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
        return {
            ...state,
            selectedScopeId: (<ItemSelectedAction>action).selectedItemId
        }

        case COST_ELEMENT_INTPUT_SELECT_COST_BLOCK:
            return {
                ...state,
                selectedCostBlockId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_ELEMENT_INTPUT_SHOW_LOSE_CHANGES_WARNING:
            return showDataLoseWarning(state, <ShowDataLoseWarningAction>action)

        case COST_ELEMENT_INTPUT_HIDE_LOSE_CHANGES_WARNING:
            return hideDataLoseWarning(state, action)

        case COST_ELEMENT_INTPUT_LOSE_CHANGES:
            return loseChanges(state, action)

        default:
            return state;
    }
}

