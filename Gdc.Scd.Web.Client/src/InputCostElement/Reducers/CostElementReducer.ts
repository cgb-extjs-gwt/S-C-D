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
import { costBlockInputReducer } from "./CostBlockInputReducer";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";

const createMap = <T extends NamedId>(array: T[]) => {
    const map = new Map<string, T>();

    array.forEach(item => map.set(item.id, item));

    return map;
}

// const getVisibleCostBlockIds = (costBlockMetas: CostBlockMeta[], selectedApplicationId: string) => {
//     return costBlockMetas.filter(costBlockMeta => costBlockMeta.applicationIds.includes(selectedApplicationId))
//                          .map(costBlockMeta => costBlockMeta.id);
// }

// const getVisibleCostElementIds = (costElementMetas: CostElementMeta[], selectedScopeId: string) => {
//     return costElementMetas.filter(costElementMeta => costElementMeta.scopeId === selectedScopeId)
//                            .map(costElementMeta => costElementMeta.id);
// }

const initSuccess: Reducer<CostElementInputState, PageAction<CostElementInputDto>> = (state, action) => {
    const { applications, scopes, costBlockMetas, countries, inputLevels } = action.data;
    const selectedApplicationId = applications[0].id;
    const selectedScopeId = scopes[0].id;
    // const selectedCountryId = countries[0].id;
    // const visibleCostBlockIds = getVisibleCostBlockIds(costBlockMetas, selectedApplicationId);

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
            // costBlocksInputs: costBlockMetas.map(costBlockMeta => (<CostBlockInputState>{
            //     costBlockId: costBlockMeta.id,
            //     selectedCountryId,
            //     costElement: {
            //         selectedItemId: null,
            //         list: costBlockMeta.costElements.map(costElementMeta => (<CostElementInput>{
            //             costElementId: costElementMeta.id
            //         }))
            //     },
            //     visibleCostElementIds: getVisibleCostElementIds(costBlockMeta.costElements, selectedScopeId),
            //     inputLevel:{
            //         selectedId: null,
            //         filter: null
            //     },
            //     editItems: null
            // })),
            // visibleCostBlockIds,
            // selectedCostBlockId: visibleCostBlockIds[0]
        } 
        : state;
}

// const selectApplication: Reducer<CostElementInputState, ItemSelectedAction> = (state, action) => {
//     const visibleCostBlockIds = getVisibleCostBlockIds(
//         Array.from(state.costBlockMetas.values()), 
//         action.selectedItemId);

//     const selectedCostBlockId = visibleCostBlockIds.includes(state.selectedCostBlockId)
//         ? state.selectedCostBlockId
//         : visibleCostBlockIds[0];

//     return {
//         ...state,
//         selectedApplicationId: action.selectedItemId,
//         visibleCostBlockIds,
//         selectedCostBlockId
//     }
// }

// const selectScope: Reducer<CostElementInputState, ItemSelectedAction> = (state, action) => {
//     return {
//         ...state,
//         selectedScopeId: action.selectedItemId,
//         costBlocksInputs: state.costBlocksInputs.map(costBlockInput => {
//             const costBlockMeta = state.costBlockMetas.get(costBlockInput.costBlockId);
//             const visibleCostElementIds = getVisibleCostElementIds(
//                 costBlockMeta.costElements, 
//                 action.selectedItemId);

//             return {
//                 ...costBlockInput,
//                 visibleCostElementIds
//             };
//         })
//     }
// }

export const costElementInputReducer: Reducer<CostElementInputState, Action<string>> = (state = <CostElementInputState>{}, action) => {
    switch(action.type) {
        case PAGE_INIT_SUCCESS:
            return initSuccess(state, <PageAction<CostElementInputDto>>action);

        case COST_ELEMENT_INTPUT_SELECT_APPLICATION:
            //return selectApplication(state, <ItemSelectedAction>action);
            return {
                ...state,
                selectedCostBlockId: (<ItemSelectedAction>action).selectedItemId
            }

        case COST_ELEMENT_INTPUT_SELECT_SCOPE:
            //return selectScope(state, <ItemSelectedAction>action);
            return {
                ...state,
                selectedScopeId: (<ItemSelectedAction>action).selectedItemId
            }

        // case COST_ELEMENT_INTPUT_SELECT_COST_BLOCK:
        //     return {
        //         ...state,
        //         selectedCostBlockId: (<ItemSelectedAction>action).selectedItemId
        //     }

        default:
            return state;
    }
}

