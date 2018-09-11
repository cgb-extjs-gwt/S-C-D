import { connect } from "react-redux";
import { QualityGateErrorProps, QualityGateErrorActions } from "../../QualityGate/Components/QualityGateErrorView";
import { QualityGateErrorContainerProps } from "../../QualityGate/Components/QualityGateErrorContainer";
import { CommonState } from "../../Layout/States/AppStates";
import { saveEditItemsToServer, resetErrors } from "../Actions/CostBlockActions";
import { QualityGateErrorWindow } from "../../QualityGate/Components/QualityGateErrorWindow";

export const QualityGateWindowContainer = 
    connect<QualityGateErrorContainerProps, QualityGateErrorActions, QualityGateErrorContainerProps, CommonState>(
        (state, props) => props,
        (dispatch, { costBlockId, onSave, onCancel }) => ({
            onSave: (explanationMessage) => dispatch(
                saveEditItemsToServer(costBlockId, { 
                    qualityGateErrorExplanation: explanationMessage,
                    isApproving: true,
                    hasQualityGateErrors: true
                })
            ),
            onCancel: () => dispatch(resetErrors(costBlockId))
        })
    )(QualityGateErrorWindow)