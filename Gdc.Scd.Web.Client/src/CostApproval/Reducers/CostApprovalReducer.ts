import { COST_APPROVAL_PAGE } from "../Constants/CostApprovalConstants";
import { buildFilterReducer } from "../../Approval/Reducers/FilterReducer";
import { buildApprovalReducer } from "../../Approval/Reducers/ApprovalReducerBuilder";
import { ApprovalBundleState } from "../../Approval/States/ApprovalState";

const filterReducer = buildFilterReducer(ApprovalBundleState.Approving)

export const costApprovalReducer = buildApprovalReducer(COST_APPROVAL_PAGE, filterReducer)