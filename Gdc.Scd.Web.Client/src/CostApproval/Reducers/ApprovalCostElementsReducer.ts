import { combineReducers, ReducersMapObject } from "redux";
import { bundleListReducer } from "./BundleListReducer";
import { bundleFilterReducer } from "./BundleFilterReducer";
import { buildPageReducer } from "../../Common/Helpers/ReducerHeper";

export const buildApprovalCostElementsReducer = (pageName: string, additionalReducers: ReducersMapObject = {}) => {
    const approvalCostElementsReducer = combineReducers({
        bundles: bundleListReducer,
        filter: bundleFilterReducer,
        ...additionalReducers
    });

    return buildPageReducer(pageName, approvalCostElementsReducer);
}