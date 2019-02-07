import { CommonState } from "../../Layout/States/AppStates";
import { connectAdvanced } from "react-redux";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { BundleListLayoutProps } from "../../Approval/Components/BundleListLayout";
import { OwnApprovalBundleFilter, getOwnBundles } from "../../Approval/Services/ApprovalService";
import { buildBundleFilter } from "../../Approval/Helpers/FilterHelper";
import { OWN_COST_APPROVAL_PAGE } from "../Constants/CostOwnApprovalConstants";
import { loadBundles } from "../../Approval/Actions/BundleListActions";
import { OwnApprovalLayout } from "./OwnApproveLayout";

export const OwnApproveLayoutContainer = connectAdvanced<CommonState, BundleListLayoutProps, {}>(
    dispatch => ({ pages: { ownCostApproval } }) => ({
        bundles: ownCostApproval.bundles,
        onRefresh: () => {
            const filter: OwnApprovalBundleFilter = {
                ...buildBundleFilter(ownCostApproval.filter),
                state: ownCostApproval.filter.selectedState
            };

            handleRequest(
                getOwnBundles(filter).then(bundles => {
                    dispatch(loadBundles(OWN_COST_APPROVAL_PAGE, bundles))
                })
            );
        }
    })
)(OwnApprovalLayout)