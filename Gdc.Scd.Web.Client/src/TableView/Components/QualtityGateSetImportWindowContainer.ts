import { connect } from "react-redux";
import { QualityGateToolbarActions } from "../../QualityGate/Components/QualityGateToolbar";
import { CommonState } from "../../Layout/States/AppStates";
import { QualityGateSetWindowProps } from "./QualityGateSetWindow";
import { importAfterQualityGateExplanation } from "../Actions/TableViewActionsAsync";
import { QualtityGateSetWindowContainer } from "./QualtityGateSetWindowContainer";

export const QualtityGateSetImportWindowContainer =
    connect<{}, QualityGateToolbarActions, QualityGateSetWindowProps, CommonState>(
        () => ({}),
        dispatch => ({
            onSave: explanationMessage => dispatch(importAfterQualityGateExplanation(explanationMessage))
        })
    )(QualtityGateSetWindowContainer)