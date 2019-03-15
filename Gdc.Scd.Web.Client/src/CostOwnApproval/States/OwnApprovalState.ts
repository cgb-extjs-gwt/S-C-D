import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";
import { ApprovalCostElementsLayoutState } from "../../Approval/States/ApprovalState";

export interface OwnApprovalCostElementsLayoutState extends ApprovalCostElementsLayoutState {
    qualityGateErrors: BundleDetailGroup[]
}