import { BundleFilterStates } from "./BundleFilterStates";
import { BundleListState } from "./BundleListState";

export interface ApprovalCostElementsLayoutState {
    bundles: BundleListState
    filter: BundleFilterStates
}