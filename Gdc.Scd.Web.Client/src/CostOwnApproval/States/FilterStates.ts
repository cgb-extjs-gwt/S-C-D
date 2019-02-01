import { FilterState, ApprovalBundleState } from "../../Approval/States/ApprovalState";

export interface OwnApprovalFilterState extends FilterState {
    selectedState: ApprovalBundleState
}

