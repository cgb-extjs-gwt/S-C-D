import { CommonState } from "../../Layout/States/AppStates";
import { connect } from "react-redux";
import FilterBundlesView, { FilterApprovalProps, ApprovalFilterActions } from "./FilterBundlesView";
import { init } from "../Actions/CostApprovalFilterActions";


export const FilterBundleContainer = connect<FilterApprovalProps, ApprovalFilterActions, {}, CommonState>
(
    state => {
        
        const applications = state.app.appMetaData ? state.app.appMetaData : []
        const selectedApplicationId = state.pages.costApproval.selectedApplicationId

        return <FilterApprovalProps>{
            application: {
                selectedItemId: selectedApplicationId,
                list: applications
            }
        }
    },
    dispatch => (<ApprovalFilterActions>{
        
    })
)(FilterBundlesView)