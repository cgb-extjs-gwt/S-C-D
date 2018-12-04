import { ApprovalCostElementsLayoutState } from "./ApprovalCostElementsLayoutState";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";

export interface OwnApprovalCostElementsLayoutState extends ApprovalCostElementsLayoutState {
    qualityGateErrors: BundleDetailGroup[]
}