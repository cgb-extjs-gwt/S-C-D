import { Reducer, Action } from "redux";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { filterReducer } from "../../Approval/Reducers/FilterReducer";
import { OwnApprovalFilterState } from "../States/FilterStates";
import { OWN_COST_APPROVAL_SELECT_STATE } from "../Actions/FilterActions";
import { ApprovalBundleState } from "../../Approval/States/ApprovalState";

const initState = () => ({
    ...filterReducer(undefined, { type: null }),
    selectedState: ApprovalBundleState.Saved
})

const selectState: Reducer<OwnApprovalFilterState, ItemSelectedAction<ApprovalBundleState>> = (state, { selectedItemId }) => ({
    ...state,
    selectedState: selectedItemId
})

export const ownApprovalFilterReducer: Reducer<OwnApprovalFilterState, Action<string>> = (state = initState(), action) => {
    switch(action.type) {
        case OWN_COST_APPROVAL_SELECT_STATE:
            return selectState(state, <ItemSelectedAction<ApprovalBundleState>>action);

        default:
            return filterReducer(state, action) as OwnApprovalFilterState;
    }
}