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
        (dispatch, { bundleId, onHandled }) => {
            const handlePromise = (promise: Promise<any>) => promise.then(() => onHandled && onHandled());

            return  {
                onApprove: () => handlePromise(CostApprovalService.approve(bundleId)),
                onSendBackToRequestor: message => handlePromise(CostApprovalService.reject(bundleId, message))
            }
        }
    )(ApproveRejectComponent)