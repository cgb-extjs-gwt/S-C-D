import { connect } from "react-redux";
import { QualityGateErrorProps, QualityGateErrorActions } from "../../QualityGate/Components/QualityGateErrorView";
import { QualityGateErrorContainerProps } from "../../QualityGate/Components/QualityGateErrorContainer";
import { CommonState } from "../../Layout/States/AppStates";
import { saveEditItemsToServer, resetErrors } from "../Actions/CostBlockActions";
import { QualityGateErrorWindow } from "../../QualityGate/Components/QualityGateErrorWindow";

export interface QualityGateWindowContainer extends QualityGateErrorContainerProps {
    applicationId: string
}

export const QualityGateWindowContainer = 
    connect<QualityGateErrorContainerProps, QualityGateErrorActions, QualityGateWindowContainer, CommonState>(
        (state, props) => props,
        (dispatch, { applicationId, costBlockId, onSave, onCancel }) => ({
            onSave: (explanationMessage) => dispatch(
                saveEditItemsToServer(applicationId, costBlockId, { 
                    qualityGateErrorExplanation: explanationMessage,
                    isApproving: true,
                    hasQualityGateErrors: true
                })
            ),
            onCancel: () => dispatch(resetErrors(applicationId, costBlockId))
        })
    )(QualityGateErrorWindow)