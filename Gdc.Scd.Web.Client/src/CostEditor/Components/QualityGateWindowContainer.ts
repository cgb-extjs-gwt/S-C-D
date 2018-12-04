import { connect } from "react-redux";
import { QualityGateErrorProps, } from "../../QualityGate/Components/QualityGateErrorView";
import { QualityGateErrorContainerProps } from "../../QualityGate/Components/QualityGateErrorContainer";
import { CommonState } from "../../Layout/States/AppStates";
import { saveEditItemsToServer, resetErrors } from "../Actions/CostBlockActions";
import { QualityGateErrorWindow, QualityGateErrorWindowProps } from "../../QualityGate/Components/QualityGateErrorWindow";
import { Position } from "../../Common/States/ExtStates";
import { QualityGateToolbarActions } from "../../QualityGate/Components/QualityGateToolbar";

export interface QualityGateWindowContainer extends QualityGateErrorWindowProps {
    applicationId: string
    position?: Position
}

export const QualityGateWindowContainer = 
    connect<QualityGateErrorWindowProps, QualityGateToolbarActions, QualityGateWindowContainer, CommonState>(
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