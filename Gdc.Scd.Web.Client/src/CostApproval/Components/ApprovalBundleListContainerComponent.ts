import { connect } from "react-redux";
import { ApprovalBundleListProps, ApprovalBundleListComponent, ApprovalBundleListActions } from "./ApprovalBundleListComponent";
import { CommonState } from "../../Layout/States/AppStates";
import { BundleFilter } from "../States/BundleFilter";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { ApprovalBundleState } from "../States/ApprovalBundleState";
import { PageName } from "../../Common/States/CommonStates";
import { ApprovalCostElementsLayoutState } from "../States/ApprovalCostElementsLayoutState";
import { loadBundlesByFilter } from "../Actions/BundleListActions";

// export const ApprovalBundleListContainerComponent = connect<ApprovalBundleListProps, {}, ApprovalBundleListProps, CommonState>(
//     (state, { buildChildrenBundleItem, state: bundleState })  => {
//         let filter: BundleFilter = null;

//         const applyFilter = state.pages.costApproval.applyFilter;

//         if (applyFilter) {
//             filter = {
//                 dateTimeFrom: applyFilter.startDate || null,
//                 dateTimeTo: applyFilter.endDate || null,
//                 applicationIds: applyFilter && applyFilter.selectedApplicationId ? [ applyFilter.selectedApplicationId ] : null,
//                 costBlockIds: applyFilter.selectedCostBlockIds || null,
//                 costElementIds: applyFilter.selectedCostElementIds ? applyFilter.selectedCostElementIds.map(el => el.element) : null
//             }
//         }

//         return { 
//             filter,
//             buildChildrenBundleItem,
//             state: bundleState
//         }
//     }
// )(ApprovalBundleListComponent)

export interface ApprovalBundleListContainerProps extends PageName {
    approvalBundleState: ApprovalBundleState
    buildChildrenBundleItem?(bundle: ApprovalBundle, onHandled: () => void): any
}

export const ApprovalBundleListContainerComponent = connect<ApprovalBundleListProps, ApprovalBundleListActions, ApprovalBundleListContainerProps & any, CommonState>(
    (state, { buildChildrenBundleItem, approvalBundleState, pageName })  => {
        const { bundles } = <ApprovalCostElementsLayoutState>state.pages[pageName]
        
        return {
            bundles
        };
    },
    (dispatch, { pageName, approvalBundleState }) => ({
        reloadBundles: () => loadBundlesByFilter(pageName, approvalBundleState)
    })
)(ApprovalBundleListComponent)