import { CostEditorState } from "../States/CostEditorStates";
import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { showDataLoseWarning } from "../Actions/CostEditorActions";
import { Dispatch } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";

const hasUnsavedChanges = (state: CostEditorState) => 
    !state.costBlocks.every(costBlock => !costBlock.edit.editedItems || costBlock.edit.editedItems.length === 0)

export const losseDataCheckHandlerAction = (
    handler: (dispatch: Dispatch, state: CostEditorState) => void
) => 
    asyncAction<CommonState>(
        (dispatch, getState, asyncAction) => {
            const state = getState();

            if (hasUnsavedChanges(state.pages.costEditor)) {
                dispatch(showDataLoseWarning(asyncAction));
            } else {
                handler(dispatch, state.pages.costEditor)
            }
        }
    )

export const losseDataCheckAction = (action: Action<string>) => losseDataCheckHandlerAction(
    dispatch => dispatch(action)
)