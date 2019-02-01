import { COST_APPROVAL_PAGE } from "../Constants/CostApprovalConstants";
import { filterReducer } from "../../Approval/Reducers/FilterReducer";
import { buildApprovalReducer } from "../../Approval/Reducers/ApprovalReducerBuilder";

export const costApprovalReducer = buildApprovalReducer(COST_APPROVAL_PAGE, filterReducer)