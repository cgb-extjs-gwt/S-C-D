import { connect } from "react-redux";
import { HistoryValuesGridViewProps, HistoryValuesGridView } from "./HistoryValuesGridView";
import { CommonState } from "../../Layout/States/AppStates";
import { CostEditorState } from "../States/CostEditorStates";
import { buildCostEditorContext } from "../Helpers/CostEditorHelpers";
import { buildMvcUrl } from "../../Common/Services/Ajax";
import { COST_EDITOR_CONTROLLER_NAME, buildGetCostBlockHistoryValueDtoUrl } from "../Services/CostEditorServices";

export interface HistoryValuesGridContainerProps {
    editItemId: string
}

export const HistoryValuesGridContainer = 
    connect<HistoryValuesGridViewProps, {}, HistoryValuesGridContainerProps, CommonState>(
        (state, { editItemId }) => {
            const costEditorState = state.pages.costEditor;
            const context = buildCostEditorContext(costEditorState);
            const dataLoadUrl = buildGetCostBlockHistoryValueDtoUrl(context, editItemId);

            return {
                dataLoadUrl: dataLoadUrl
            }
        }
    )(HistoryValuesGridView);