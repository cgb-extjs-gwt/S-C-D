import { Reducer, Action } from "redux";
import { COST_APPROVAL_PAGE,
         COST_APPROVAL_SELECT_APPLICATION,
         COST_APPROVAL_SELECT_COST_BLOCKS,
         COST_APPROVAL_SELECT_COST_ELEMENTS,
         COST_APPROVAL_SELECT_PERIOD } from '../Actions/CostApprovalFilterActions';
import { BundleFilterStates } from '../States/BundleFilterStates'
import { PageInitAction, APP_PAGE_INIT } from "../../Layout/Actions/AppActions";

const defaultBundleFilterState : BundleFilterStates = {
    selectedApplicationId: "hardware"
};

export const bundleFilterReducer: Reducer<BundleFilterStates, Action<string>> = (state = defaultBundleFilterState, action) => {
    switch(action.type){
       default:
            return state;
    }

}
