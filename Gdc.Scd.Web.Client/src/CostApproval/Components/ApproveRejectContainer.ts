import { connect } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { ApproveRejectActions, ApproveRejectComponent } from "./ApproveReject";
import { ApproveRejectProps } from "../../Approval/Components/Props/ApproveRejectProps";
import { approve, reject } from "../../Approval/Services/ApprovalService";

export const ApproveRejectContainerComponent = 
    connect<{}, ApproveRejectActions, ApproveRejectProps, CommonState>(
        null,
        (dispatch, { bundleId, onHandled }) => {
            const handlePromise = (promise: Promise<any>) => handleRequest(
                promise.then(() => onHandled && onHandled())
            );

            return  {
                onApprove: () => handlePromise(approve(bundleId)),
                onSendBackToRequestor: message => handlePromise(reject(bundleId, message))
            }
        }
    )(ApproveRejectComponent)