import { connect } from "react-redux";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { CommonState } from "../../Layout/States/AppStates";
import { loadQualityGateResult } from "../Actions/QualityGateActions";
import { OwnApproveRejectActions, OwnApproveRejectComponent } from "./OwnApproveReject";
import { ApproveRejectProps } from "../../Approval/Components/Props/ApproveRejectProps";
import { sendForApproval, reject } from "../../Approval/Services/ApprovalService";

export const OwnApproveRejectContainerComponent = 
    connect<{}, OwnApproveRejectActions, ApproveRejectProps, CommonState>(
        null,
        (dispatch, { bundleId, onHandled = () => {} }) => ({
            onApprove: () => handleRequest(
                sendForApproval(bundleId).then(qualityGateResult => {
                    dispatch(loadQualityGateResult(qualityGateResult.errors))

                    if (qualityGateResult.errors.length == 0) {
                        onHandled();
                    }
                })
            ),
            onReject: () => handleRequest(
                reject(bundleId).then(() => onHandled())
            ),
            onQualityGateHandled: () => onHandled()
        })
    )(OwnApproveRejectComponent)