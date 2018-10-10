import { connect } from "react-redux";
import { ApprovalBundleListProps, ApprovalBundleListComponent, ApprovalBundleListActions } from "./ApprovalBundleListComponent";
import { CommonState } from "../../Layout/States/AppStates";
import { BundleFilter } from "../States/BundleFilter";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { ApprovalBundleState } from "../States/ApprovalBundleState";
import { PageName } from "../../Common/States/CommonStates";
import { ApprovalCostElementsLayoutState } from "../States/ApprovalCostElementsLayoutState";
import { loadBundlesByFilter } from "../Actions/BundleListActions";

export interface ApprovalBundleListContainerProps extends PageName {
    title: string
    approvalBundleState: ApprovalBundleState
    isCheckColumnsVisible: boolean
    buildChildrenBundleItem?(bundle: ApprovalBundle, onHandled: () => void): any
}

export const ApprovalBundleListContainerComponent = connect<ApprovalBundleListProps, ApprovalBundleListActions, ApprovalBundleListContainerProps & any, CommonState>(
    (state, { buildChildrenBundleItem, approvalBundleState, pageName, isCheckColumnsVisible })  => {
        const { bundles } = <ApprovalCostElementsLayoutState>state.pages[pageName]
        
        return {
            bundles,
            isCheckColumnsVisible
        };
    },
    (dispatch, { pageName, approvalBundleState, title }) => ({
        onReloadBundles: () => dispatch(loadBundlesByFilter(pageName, approvalBundleState))
    })
)(ApprovalBundleListComponent)