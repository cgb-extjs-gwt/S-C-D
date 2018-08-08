import { connect } from "react-redux";
import { ApprovalBundleListProps, ApprovalBundleListComponent } from "./ApprovalBundleListComponent";
import { CommonState } from "../../Layout/States/AppStates";

export const ApprovalBundleListContainerComponent = connect<ApprovalBundleListProps, {}, any, CommonState>(
    state => ({
        filter: {
            dateTimeFrom: state.pages.costApproval.filter.startDate,
            dateTimeTo: state.pages.costApproval.filter.endDate,
            applicationIds: [state.pages.costApproval.filter.selectedApplicationId],
            costBlockIds: state.pages.costApproval.filter.selectedCostBlockIds,
            costElementIds: state.pages.costApproval.filter.selectedCostElementIds.map(el => el.element)
        },
        flex: 2
    })
)(ApprovalBundleListComponent)