import { combineReducers } from "redux";
import { bundleListReducer } from "./BundleListReducer";
import { bundleFilterReducer } from "./BundleFilterReducer";
import { buildPageReducer } from "../../Common/Helpers/ReducerHeper";

const approvalCostElementsReducer = combineReducers({
    bundles: bundleListReducer,
    filter: bundleFilterReducer
});

export const buildApprovalCostElementsReducer = (pageName: string) => buildPageReducer(pageName, approvalCostElementsReducer);