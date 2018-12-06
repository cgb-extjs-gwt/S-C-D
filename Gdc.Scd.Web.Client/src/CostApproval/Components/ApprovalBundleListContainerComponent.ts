import { connect } from "react-redux";
import { ApprovalBundleListProps, ApprovalBundleListComponent, ApprovalBundleListActions } from "./ApprovalBundleListComponent";
import { CommonState } from "../../Layout/States/AppStates";
import { BundleFilter } from "../States/BundleFilter";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { ApprovalBundleState } from "../States/ApprovalBundleState";
import { PageName } from "../../Common/States/CommonStates";
import { ApprovalCostElementsLayoutState } from "../States/ApprovalCostElementsLayoutState";
import { Action } from "redux";

export interface ApprovalBundleListContainerProps extends PageName {
    title: string
    approvalBundleState: ApprovalBundleState
    isCheckColumnsVisible: boolean
    flex: number
    buildChildrenBundleItem?(bundle: ApprovalBundle, onHandled: () => void): any
    buildReloadBundlesAction(pageName: string, approvalBundleState: ApprovalBundleState): Action<string>
}

export const ApprovalBundleListContainerComponent = connect<ApprovalBundleListProps, ApprovalBundleListActions, ApprovalBundleListContainerProps, CommonState>(
    (state, { buildChildrenBundleItem, approvalBundleState, pageName, isCheckColumnsVisible, flex })  => {
        const { bundles } = <ApprovalCostElementsLayoutState>state.pages[pageName]
        
        return {
            bundles,
            flex,
            isCheckColumnsVisible
        };
    },
    (dispatch, { pageName, approvalBundleState, title, buildReloadBundlesAction }) => ({
        onReloadBundles: () => dispatch(buildReloadBundlesAction(pageName, approvalBundleState))
    })
)(ApprovalBundleListComponent)