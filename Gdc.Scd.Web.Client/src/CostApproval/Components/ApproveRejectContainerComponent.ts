import { connect } from "react-redux";
import { ApproveRejectActions, ApproveRejectComponent } from "./ApproveRejectComponent";
import { CommonState } from "../../Layout/States/AppStates";
import * as CostApprovalService from "../Services/CostApprovalService"

export interface ApproveRejectContainerProps {
    bundleId: number,
    onHandled?()
}

export const ApproveRejectContainerComponent = 
    connect<{}, ApproveRejectActions, ApproveRejectContainerProps, CommonState>(
        null,
        (dispatch, { bundleId, onHandled }) => ({
            onApprove: () => {
                CostApprovalService.approve(bundleId)
                onHandled && onHandled();
            },
            onSendBackToRequestor: message => {
                CostApprovalService.reject(bundleId, message);
                onHandled && onHandled();
            }
        })
    )(ApproveRejectComponent)