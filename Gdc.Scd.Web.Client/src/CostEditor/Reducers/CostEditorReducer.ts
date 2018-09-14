import { Action, Reducer } from "redux";
import { CostEditorState, CostEditortData, CostBlockMeta, CostElementMeta, InputLevelMeta } from "../States/CostEditorStates";
import { 
    COST_EDITOR_PAGE, 
    COST_EDITOR_SELECT_APPLICATION, 
    COST_EDITOR_SELECT_COST_BLOCK,
    COST_EDITOR_HIDE_LOSE_CHANGES_WARNING,
    COST_EDITOR_LOSE_CHANGES,
    COST_EDITOR_SHOW_LOSE_CHANGES_WARNING,
    ShowDataLoseWarningAction
} from "../Actions/CostEditorActions";
import { CostBlockState, CostElementState } from "../States/CostBlockStates";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { NamedId } from "../../Common/States/CommonStates";
import { APP_PAGE_INIT, PageInitAction } from "../../Layout/Actions/AppActions";
import { CountryInputLevelName } from "../../Common/Constants/MetaConstants";

const createMap = <T extends NamedId>(array: T[]) => {
    const map = new Map<string, T>();

    array.forEach(item => map.set(item.id, item));

    return map;
}

const initSuccess: Reducer<CostEditorState, PageInitAction<CostEditortData>> = (state, action) => {
    const { applications, costBlocks } = action.data;
    const selectedApplicationId = applications[0].id;
    const costBlockMetas = costBlocks.map(costBlock => (<CostBlockMeta>{
        ...costBlock,
        costElements: costBlock.costElements.map(costElement => ({
            ...costElement,
            inputLevels: costElement.inputLevels.map((inputLevel, index) => (<InputLevelMeta>{
                ...inputLevel,
                levelNumer: index,
                isFilterLoading: index > 0 && costElement.inputLevels[index - 1].id != CountryInputLevelName
            }))
        }))
    }))

    return action.pageId === COST_EDITOR_PAGE 
        ? {
            ...state,
            applications: createMap(applications),
            regions: createMap([]),
            costBlockMetas: createMap(costBlockMetas),
            selectedApplicationId
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
        case APP_PAGE_INIT:
            return initSuccess(state, <PageInitAction<CostEditortData>>action);

        case COST_EDITOR_SELECT_APPLICATION:
            return {
                ...state,
                selectedApplicationId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_EDITOR_SELECT_COST_BLOCK:
            return {
                ...state,
                selectedCostBlockId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_EDITOR_SHOW_LOSE_CHANGES_WARNING:
            return showDataLoseWarning(state, <ShowDataLoseWarningAction>action)

        case COST_EDITOR_HIDE_LOSE_CHANGES_WARNING:
            return hideDataLoseWarning(state, action)

        case COST_EDITOR_LOSE_CHANGES:
            return loseChanges(state, action)

        default:
            return state;
    }
}

