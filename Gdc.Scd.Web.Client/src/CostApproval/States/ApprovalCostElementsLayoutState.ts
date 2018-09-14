import { BundleFilterStates } from "./BundleFilterStates";
import { ApprovalBundle } from "./ApprovalBundle";

export interface ApprovalCostElementsLayoutState {
    bundles: ApprovalBundle[]
    filter: BundleFilterStates
}
