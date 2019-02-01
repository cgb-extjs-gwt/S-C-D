import { connect } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { PageItemSelectedAction } from "../../Common/Actions/CommonActions";
import { OwnApproveBundlesFilterProps, OwnApproveBundlesFilterActions, OwnApproveBundlesFilter } from "./Filter";
import { OWN_COST_APPROVAL_SELECT_STATE } from "../Actions/FilterActions";
import { ApprovalBundleState } from "../../Approval/States/ApprovalState";

export const OwnApproveBundlesFilterContainer = connect<OwnApproveBundlesFilterProps, OwnApproveBundlesFilterActions, {}, CommonState>(
    ({ pages: { ownCostApproval } }) => ({
        selectedState: ownCostApproval.filter.selectedState
    }),
    dispatch => ({
        onStateSelect: approvalState => dispatch(<PageItemSelectedAction<ApprovalBundleState>>{ 
            type: OWN_COST_APPROVAL_SELECT_STATE,
            selectedItemId: approvalState
        })
    })
)(OwnApproveBundlesFilter)