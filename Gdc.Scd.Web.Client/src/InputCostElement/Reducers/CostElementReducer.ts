import { Action, Reducer } from "redux";
import { PageState } from "../../Layout/States/PageStates";
import { CommonAction } from "../../Common/CommonAction";
import { CostElementInputState, CostElementInputDto, CostBlockMeta, CostElementMeta } from "../States/CostElementState";
import { PageAction, PAGE_INIT_SUCCESS } from "../../Layout/Actions/PageActions";
import { 
    COST_ELEMENT_INTPUT_PAGE, 
    COST_ELEMENT_INTPUT_SELECT_APPLICATION, 
    COST_ELEMENT_INTPUT_SELECT_SCOPE,
    COST_ELEMENT_INTPUT_SELECT_COST_BLOCK
} from "../Actions/InputCostElementActions";
import { SelectList } from "../../Common/States/SelectList";
import { NamedId } from "../../Common/States/NamedId";
import { CostBlockInputState, CostElementInput } from "../States/CostBlock";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";

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

export const costElementInputReducer: Reducer<CostElementInputState, Action<string>> = (state = <CostElementInputState>{}, action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return initSuccess(state, <PageAction<CostElementInputDto>>action);

        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
            return state.isDataLossWarningDisplayed 
                ?  {
                    ...state,
                    selectedApplicationId: (<ItemSelectedAction>action).selectedItemId
                }
                : state;

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
            return {
                ...state,
                selectedScopeId: (<ItemSelectedAction>action).selectedItemId
            }

        default:
            return state;
    }
}

