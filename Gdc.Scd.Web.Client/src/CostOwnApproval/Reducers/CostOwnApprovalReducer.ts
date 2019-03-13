import { buildApprovalReducer } from "../../Approval/Reducers/ApprovalReducerBuilder";
import { OWN_COST_APPROVAL_PAGE } from "../Constants/CostOwnApprovalConstants";
import { qualityGateErrorsReducer } from "./QualityGateReducer";
import { buildFilterReducer } from "../../Approval/Reducers/FilterReducer";
import { ApprovalBundleState } from "../../Approval/States/ApprovalState";

const filterReducer = buildFilterReducer(ApprovalBundleState.Saved)

export const costOwnApprovalReducer = buildApprovalReducer(OWN_COST_APPROVAL_PAGE, filterReducer, {
    qualityGateErrors: qualityGateErrorsReducer
})