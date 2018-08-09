import { connect } from "react-redux";
import { ApprovalBundleListProps, ApprovalBundleListComponent } from "./ApprovalBundleListComponent";
import { CommonState } from "../../Layout/States/AppStates";
import { BundleFilter } from "../States/BundleFilter";

export const ApprovalBundleListContainerComponent = connect<ApprovalBundleListProps, {}, any, CommonState>(
    state => {
        let filter: BundleFilter = null;

        const applyFilter = state.pages.costApproval.applyFilter;

        if (applyFilter) {
            filter = {
                dateTimeFrom: applyFilter.startDate || null,
                dateTimeTo: applyFilter.endDate || null,
                applicationIds: applyFilter && applyFilter.selectedApplicationId ? [ applyFilter.selectedApplicationId ] : null,
                costBlockIds: applyFilter.selectedCostBlockIds || null,
                costElementIds: applyFilter.selectedCostElementIds ? applyFilter.selectedCostElementIds.map(el => el.element) : null
            }
        }

        return { filter }
    }
)(ApprovalBundleListComponent)