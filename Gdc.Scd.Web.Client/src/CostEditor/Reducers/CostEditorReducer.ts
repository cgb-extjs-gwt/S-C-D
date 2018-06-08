import { Action, Reducer } from "redux";
import { PageState } from "../../Layout/States/PageStates";
import { CostEditorState, CostEdirotDto, CostBlockMeta, CostElementMeta } from "../States/CostEditorStates";
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
} from "../Actions/CostEditorActions";
import { CostBlockState, CostElementState } from "../States/CostBlockStates";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { NamedId } from "../../Common/States/CommonStates";

const createMap = <T extends NamedId>(array: T[]) => {
    const map = new Map<string, T>();

    array.forEach(item => map.set(item.id, item));

    return map;
}

const initSuccess: Reducer<CostEditorState, PageAction<CostEdirotDto>> = (state, action) => {
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

const defaultState = () => (<CostEditorState>{
    dataLossInfo: {
        isWarningDisplayed: false,
        action: null,
    }
})

const showDataLoseWarning: Reducer<CostEditorState, ShowDataLoseWarningAction> = (state, action) => ({
    ...state,
    dataLossInfo: {
        ...state.dataLossInfo,
        isWarningDisplayed: true,
        action: action.dataLoseAction
    }
})

const hideDataLoseWarning: Reducer<CostEditorState> = state => ({
    ...state,
    dataLossInfo: {
        ...state.dataLossInfo,
        isWarningDisplayed: false,
        action: null
    }
})

const loseChanges: Reducer<CostEditorState, Action<string>> = state => ({
    ...state,
    costBlocks: state.costBlocks.map(costBlock => (<CostBlockState>{
        ...costBlock,
        edit: {
            ...costBlock.edit,
            editedItems: []
        }
    })),
    dataLossInfo: {
        ...state.dataLossInfo,
    }
})


export const costEditorReducer: Reducer<CostEditorState, Action<string>> = (state = defaultState(), action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return initSuccess(state, <PageAction<CostEdirotDto>>action);

        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
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

