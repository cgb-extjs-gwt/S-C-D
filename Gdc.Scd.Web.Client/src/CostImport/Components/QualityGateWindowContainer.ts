import { connect } from "react-redux";
import { QualityGateErrorWindow, QualityGateErrorWindowProps } from "../../QualityGate/Components/QualityGateErrorWindow";
import { QualityGateToolbarActions } from "../../QualityGate/Components/QualityGateToolbar";
import { CommonState } from "../../Layout/States/AppStates";
import { Position } from "../../Common/States/ExtStates";
import { importExcel } from "../Actions/CostImportAsyncAtions";
import { loadQualityGateErrors } from "../Actions/CostImportActions";

export interface QualityGateWindowContainerProps {
    position?: Position
}

export const QualityGateWindowContainer = 
    connect<QualityGateErrorWindowProps, QualityGateToolbarActions, QualityGateWindowContainerProps, CommonState>(
        ({ pages: { costImport } }, { position }) => ({
            costBlockId: costImport.costBlockId,
            costElementId: costImport.costElementId,
            errors: costImport.qualityGateErrors,
            position
        }),
        dispatch => ({
            onSave: (explanationMessage) => dispatch(
                importExcel({ 
                    isApproving: true,
                    hasQualityGateErrors: true,
                    qualityGateErrorExplanation: explanationMessage
                })
            ),
            onCancel: () => dispatch(loadQualityGateErrors(null))
        })
    )(QualityGateErrorWindow)