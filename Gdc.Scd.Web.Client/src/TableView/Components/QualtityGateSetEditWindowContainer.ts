import { connect } from "react-redux";
import { QualityGateToolbarActions } from "../../QualityGate/Components/QualityGateToolbar";
import { CommonState } from "../../Layout/States/AppStates";
import { QualityGateSetWindowProps } from "./QualityGateSetWindow";
import { saveTableViewToServer } from "../Actions/TableViewActionsAsync";
import { QualtityGateSetWindowContainer } from "./QualtityGateSetWindowContainer";

export const QualtityGateSetEditWindowContainer =
    connect<{}, QualityGateToolbarActions, QualityGateSetWindowProps, CommonState>(
        () => ({}),
        dispatch => ({
            onSave: explanationMessage => dispatch(
                saveTableViewToServer({ 
                    hasQualityGateErrors: true, 
                    isApproving: true,
                    qualityGateErrorExplanation: explanationMessage
                })
            )
        })
    )(QualtityGateSetWindowContainer)