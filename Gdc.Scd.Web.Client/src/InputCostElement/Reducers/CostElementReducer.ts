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
    COST_ELEMENT_INTPUT_LOSE_CHANGES
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

const hasUnsavedChanges = (state: CostElementInputState) => 
    !state.costBlocksInputs.every(costBlock => !costBlock.edit.editedItems || costBlock.edit.editedItems.length === 0)

const clearChanges = (costBlocks: CostBlockInputState[]) => 
    costBlocks.map(costBlock => (<CostBlockInputState>{
        ...costBlock,
        edit: {
            ...costBlock.edit,
            editedItems: []
        }
    }))

export const  buildProtectionData = <TAction extends Action<string>>(
    fn: (state: CostElementInputState, action: TAction) => CostElementInputState
) => 
    (state: CostElementInputState, action: Action<string>): CostElementInputState  => {
        let result: CostElementInputState;

        const { isWarningDisplayed, isLoseChanges } = state.dataLossInfo;

        if (isWarningDisplayed) {
            result = state;
        } else if (isLoseChanges) {
            result = { 
                ...fn(state, <TAction>action),
                costBlocksInputs: clearChanges(state.costBlocksInputs),
                dataLossInfo: {
                    ...state.dataLossInfo,
                    isLoseChanges: false
                }
            };
        } else if(hasUnsavedChanges(state)){
            result = {
                ...state,
                dataLossInfo: {
                    ...state.dataLossInfo,
                    isWarningDisplayed: true,
                    action
                }
            }
        } else {
            result = fn(state, <TAction>action);
        }

        return result;
    }
        


const selectApplication = buildProtectionData<ItemSelectedAction>(
    (state, action) => ({
        ...CostBlockReducers.selectApplication(state, action),
        selectedApplicationId: action.selectedItemId
    })
)

const selectScope = buildProtectionData<ItemSelectedAction>(
    (state, action) => ({
        ...CostBlockReducers.selectScope(state, action),
        selectedScopeId: (<ItemSelectedAction>action).selectedItemId
    })
)

const defaultState = () => (<CostElementInputState>{
    dataLossInfo: {
        isWarningDisplayed: false,
        action: null,
        isLoseChanges: false
    }
})

export const costElementInputReducer: Reducer<CostElementInputState, Action<string>> = (state = defaultState(), action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return initSuccess(state, <PageAction<CostElementInputDto>>action);

        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
            return selectApplication(state, action);

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
            return selectScope(state, action);

        case COST_ELEMENT_INTPUT_SELECT_COST_BLOCK:
            return {
                ...state,
                selectedCostBlockId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_ELEMENT_INTPUT_HIDE_LOSE_CHANGES_WARNING:
            return {
                ...state,
                dataLossInfo: {
                    ...state.dataLossInfo,
                    isWarningDisplayed: false,
                    action: null
                }
            }

        case COST_ELEMENT_INTPUT_LOSE_CHANGES:
            return {
                ...state,
                dataLossInfo: {
                    ...state.dataLossInfo,
                    isLoseChanges: true
                }
            }

        default:
            return state;
    }
}

