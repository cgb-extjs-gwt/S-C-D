import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { PageCommonAction } from "../../Common/Actions/CommonActions";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { Bundle } from "../../Approval/States/ApprovalState";

export const COST_APPROVAL_LOAD_BUNDLES = "COST_APPROVAL.LOAD_BUNDLES";

export const loadBundles = (pageName: string, bundles: Bundle[]) => (<PageCommonAction<Bundle[]>>{
    type: COST_APPROVAL_LOAD_BUNDLES,
    data: bundles,
    pageName
})