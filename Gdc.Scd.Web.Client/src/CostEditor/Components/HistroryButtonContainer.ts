import { connect } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { buildCostEditorContext } from "../Helpers/CostEditorHelpers";
import { buildGetHistoryUrl } from "../Services/CostEditorServices";
import { HistoryButtonView, HistoryButtonViewProps } from "../../History/Components/HistoryButtonView";
import { Position } from "../../Common/States/ExtStates";

export interface HistoryValuesGridContainerProps {
    editItemId: string,
    isEnabled: boolean, 
    flex: number,
    windowPosition?: Position
}

export const HistroryButtonContainer = 
    connect<HistoryButtonViewProps, {}, HistoryValuesGridContainerProps, CommonState>(
        (state, { editItemId, isEnabled, flex, windowPosition }) => {
            const costEditorState = state.pages.costEditor;
            const context = buildCostEditorContext(costEditorState);
            const dataLoadUrl = buildGetHistoryUrl(context, editItemId);

            return {
                dataLoadUrl: dataLoadUrl,
                isEnabled,
                flex,
                windowPosition
            }
        }
    )(HistoryButtonView);