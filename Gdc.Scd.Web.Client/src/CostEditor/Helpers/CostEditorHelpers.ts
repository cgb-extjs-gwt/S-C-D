import { CostEditorState } from "../States/CostEditorStates";
import { Action } from "redux";
import { PageCommonState } from "../../Layout/States/PageStates";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { showDataLoseWarning } from "../Actions/CostEditorActions";
import { Dispatch } from "react-redux";

const hasUnsavedChanges = (state: CostEditorState) => 
    !state.costBlocks.every(costBlock => !costBlock.edit.editedItems || costBlock.edit.editedItems.length === 0)

export const losseDataCheckHandlerAction = (
    handler: (dispatch: Dispatch, state: PageCommonState<CostEditorState>) => void
) => 
    asyncAction<PageCommonState<CostEditorState>>(
        (dispatch, state, asyncAction) => {
            if (hasUnsavedChanges(state.page.data)) {
                dispatch(showDataLoseWarning(asyncAction));
            } else {
                handler(dispatch, state)
            }
        }
    )

export const losseDataCheckAction = (action: Action<string>) => losseDataCheckHandlerAction(
    dispatch => dispatch(action)
)