import { ApprovalCostElementsLayoutState } from "./ApprovalCostElementsLayoutState";

export interface OwnApprovalCostElementsLayoutState extends ApprovalCostElementsLayoutState {
    qualityGateErrors: {[key: string]: any}[]
}