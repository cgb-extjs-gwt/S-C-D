import { Reducer, Action } from "redux";
import { CostBlockInputState, CostElementInput } from "../States/CostBlock";
import { PAGE_INIT_SUCCESS, PageAction } from "../../Layout/Actions/PageActions";
import { CostElementInputDto } from "../States/CostElementState";
import { COST_ELEMENT_INTPUT_PAGE } from "../Actions/InputCostElementActions";

// const initSuccess: Reducer<CostBlockInputState[], PageAction<CostElementInputDto>> = (state, action) => {
//     const { costBlockMetas } = action.data;

//     return action.pageName === COST_ELEMENT_INTPUT_PAGE
//         ? costBlockMetas.map(costBlockMeta => (<CostBlockInputState>{
//             costBlockId: costBlockMeta.id,
//             selectedCountryId,
//             costElement: {
//                 selectedItemId: null,
//                 list: costBlockMeta.costElements.map(costElementMeta => (<CostElementInput>{
//                     costElementId: costElementMeta.id
//                 }))
//             },
//             visibleCostElementIds: getVisibleCostElementIds(costBlockMeta.costElements, selectedScopeId),
//             inputLevel:{
//                 selectedId: null,
//                 filter: null
//             },
//             editItems: null
//         }))
//         : state;
// }

export const costBlockInputReducer: Reducer<CostBlockInputState[], Action<string>> = (state = [], action) => {
    switch(action.type) {
        // case PAGE_INIT_SUCCESS:
        //     return initSuccess(state, <PageAction<CostElementInputDto>>action);
        
        default:
            return state;
    }
}