import { QualityGateErrorContainerProps } from "../../QualityGate/Components/QualityGateErrorContainer";
import { connect } from "react-redux";
import { QualityGateErrorActions } from "../../QualityGate/Components/QualityGateErrorView";
import { CommonState } from "../../Layout/States/AppStates";
import { COST_APPROVAL_QUALITY_GATE_ERROR_RESET, resetQualityGateErrors } from "../Actions/QualityGateActions";
import { sendForApproval } from "../Services/CostApprovalService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { QualityGateErrorWindow } from "../../QualityGate/Components/QualityGateErrorWindow";

export interface QualityGateWindowContainerProps extends QualityGateErrorContainerProps {
    bundleId: number
}

export const QualityGateWindowContainer = 
    connect<QualityGateErrorContainerProps, QualityGateErrorActions, QualityGateWindowContainerProps, CommonState>(
        ({ pages }, props) => ({
            ...props,
            errors: pages.ownCostApproval.qualityGateErrors
        }),
        (dispatch, { bundleId, onSave, onCancel }) => ({
            onSave: explanationMessage => {
                dispatch(resetQualityGateErrors());
                
                handleRequest(
                    sendForApproval(bundleId, explanationMessage)
                );
            },
            onCancel: () => dispatch(resetQualityGateErrors())
        })
    )(QualityGateErrorWindow)