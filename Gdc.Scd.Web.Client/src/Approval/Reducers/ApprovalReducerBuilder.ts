import { combineReducers, ReducersMapObject, Reducer, Action } from "redux";
import { buildPageReducer } from "../../Common/Helpers/ReducerHeper";
import { bundleListReducer } from "./BundleListReducer";

export const buildApprovalReducer = <TFilter>(
    pageName: string, 
    filterReducer: Reducer<TFilter, Action<string>>, 
    additionalReducers: ReducersMapObject = {}
) => {
    const approvalCostElementsReducer = combineReducers({
        bundles: bundleListReducer,
        filter: filterReducer,
        ...additionalReducers
    });

    return buildPageReducer(pageName, approvalCostElementsReducer);
}