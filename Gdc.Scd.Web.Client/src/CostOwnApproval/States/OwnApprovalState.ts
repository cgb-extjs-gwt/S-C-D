import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";
import { ApprovalCostElementsLayoutState } from "../../Approval/States/ApprovalState";
import { OwnApprovalFilterState } from "./FilterStates";

export interface OwnApprovalCostElementsLayoutState extends ApprovalCostElementsLayoutState<OwnApprovalFilterState> {
    qualityGateErrors: BundleDetailGroup[]
}