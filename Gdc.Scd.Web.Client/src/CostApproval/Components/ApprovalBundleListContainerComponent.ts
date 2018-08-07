import { connect } from "react-redux";
import { ApprovalBundleListProps, ApprovalBundleListComponent } from "./ApprovalBundleListComponent";
import { CommonState } from "../../Layout/States/AppStates";

export const ApprovalBundleListContainerComponent = connect<ApprovalBundleListProps, {}, {}, CommonState>(
    state => ({
        filter: null
    })
)(ApprovalBundleListComponent)