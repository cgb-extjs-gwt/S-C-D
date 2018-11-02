import { connect } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { buildCostEditorContext } from "../Helpers/CostEditorHelpers";
import { buildGetHistoryUrl } from "../Services/CostEditorServices";
import { HistoryButtonView, HistoryButtonViewProps } from "../../History/Components/HistoryButtonView";
import { Position } from "../../History/Components/HistoryWindowView";

export interface HistoryValuesGridContainerProps {
    editItemId: string,
    isEnabled: boolean, 
    flex: number,
    windowPosition?: Position
}

export const HistroryButtonContainer = 
    connect<HistoryButtonViewProps, {}, HistoryValuesGridContainerProps, CommonState>(
        (state, { editItemId, isEnabled, flex, windowPosition }) => {
            return {
                isEnabled,
                flex,
                windowPosition,
                buidHistoryUrl: () => {
                    const costEditorState = state.pages.costEditor;
                    const context = buildCostEditorContext(costEditorState);
                    
                    return buildGetHistoryUrl(context, editItemId);
                }
            }
        }
    )(HistoryButtonView);