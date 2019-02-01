import { Reducer, Action } from "redux";
import { COST_APPROVAL_LOAD_BUNDLES } from "../Actions/BundleListActions";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { Bundle } from "../../Approval/States/ApprovalState";

export const bundleListReducer: Reducer<Bundle[], Action<string>> = (state = [], action) => {
    switch(action.type){
        case COST_APPROVAL_LOAD_BUNDLES:
            return (<CommonAction>action).data;

        default:
            return state;
    }
}