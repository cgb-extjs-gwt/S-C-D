import { connect } from "react-redux";
import { CommonState } from "../Layout/States/AppStates";
import * as Permissions from "../Common/Constants/Permissions";
import { ReportListViewProps, ReportListView } from "./ReportListView";

export const ReportListViewContainer = connect<ReportListViewProps, {}, {}, CommonState>(
    state => ({
        isVisibleHddRetentionParameter: !!state.app.userRoles.find(userRole => userRole.permissions.includes(Permissions.REPORT_HDD_RETENTION_PARAMETER)),
    })
)(ReportListView);