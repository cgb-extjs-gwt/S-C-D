import { connect } from "react-redux";
import { CalcResultViewProps, CalcResultView } from "./CalcResultView";
import { CommonState } from "../Layout/States/AppStates";
import * as Permissions from "../Common/Constants/Permissions";

export const CalcResultViewContainer = connect<CalcResultViewProps, {}, {}, CommonState>(
    state => ({
        isVisibleHddNotApproved: !!state.app.userRoles.find(userRole => userRole.permissions.includes(Permissions.CALC_RESULT_HDD_SERVICE_COST_NOT_APPROVED)),
        isVisibleSwNotApproved: !!state.app.userRoles.find(userRole => userRole.permissions.includes(Permissions.CALC_RESULT_SOFTWARE_SERVICE_COST_NOT_APPROVED)),
    })
)(CalcResultView);