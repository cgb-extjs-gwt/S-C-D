import { connect } from "react-redux";
import { OwnApproveRejectActions, OwnApproveRejectComponent } from "./OwnApproveRejectComponent";
import { ApproveRejectContainerProps } from "./ApproveRejectContainerProps";
import { CommonState } from "../../Layout/States/AppStates";
import * as CostApprovalService from "../Services/CostApprovalService"
import { loadQualityGateResult } from "../Actions/QualityGateActions";
import { handleRequest } from "../../Common/Helpers/RequestHelper";

export const OwnApproveRejectContainerComponent = 
    connect<{}, OwnApproveRejectActions, ApproveRejectContainerProps, CommonState>(
        null,
        (dispatch, { bundleId, onHandled }) => ({
            onApprove: () => handleRequest(
                    CostApprovalService.sendForApproval(bundleId).then(qualityGateResult => {
                    dispatch(loadQualityGateResult(qualityGateResult.errors))
                })
            ),
            onReject: () => handleRequest(
                CostApprovalService.reject(bundleId).then(() => onHandled && onHandled())
            )
        })
    )(OwnApproveRejectComponent)