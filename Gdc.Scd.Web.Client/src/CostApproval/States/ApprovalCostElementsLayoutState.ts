import { ApprovalBundle } from "./ApprovalBundle";
import { BundleFilterStates } from "./BundleFilterStates";

export interface ApprovalCostElementsLayoutState {
    bundles: ApprovalBundle[]
    filter: BundleFilterStates
}