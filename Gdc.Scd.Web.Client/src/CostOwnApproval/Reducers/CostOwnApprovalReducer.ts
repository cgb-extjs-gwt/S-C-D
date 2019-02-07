import { buildApprovalReducer } from "../../Approval/Reducers/ApprovalReducerBuilder";
import { OWN_COST_APPROVAL_PAGE } from "../Constants/CostOwnApprovalConstants";
import { ownApprovalFilterReducer } from "./FilterReducer";
import { qualityGateErrorsReducer } from "./QualityGateReducer";

export const costOwnApprovalReducer = buildApprovalReducer(OWN_COST_APPROVAL_PAGE, ownApprovalFilterReducer, {
    qualityGateErrors: qualityGateErrorsReducer
})