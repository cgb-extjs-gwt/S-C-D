import { CostEditorState } from "../States/CostEditorStates";
import { Action, Dispatch } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { pageInit } from "../../Layout/Actions/AppActions";
import { CommonState } from "../../Layout/States/AppStates";
import { handleRequest } from "../../Common/Helpers/RequestHelper";

export const COST_EDITOR_PAGE = 'costEditor';
export const COST_EDITOR_SELECT_APPLICATION = 'COST_EDITOR.SELECT.APPLICATION';
// export const COST_EDITOR_HIDE_LOSE_CHANGES_WARNING = 'COST_EDITOR.HIDE.LOSE_CHANGES_WARNING';
// export const COST_EDITOR_SHOW_LOSE_CHANGES_WARNING = 'COST_EDITOR.SHOW.LOSE_CHANGES_WARNING';
// export const COST_EDITOR_LOSE_CHANGES = 'COST_EDITOR.LOSE.CHANGES';

// export interface ShowDataLoseWarningAction extends Action<string> {
//     dataLoseAction: Action<string>
// }

export const selectApplication = (applicationId: string) => (<ItemSelectedAction>{
    type: COST_EDITOR_SELECT_APPLICATION,
    selectedItemId: applicationId
})

// export const showDataLoseWarning = dataLoseAction => (<ShowDataLoseWarningAction>{
//     type: COST_EDITOR_SHOW_LOSE_CHANGES_WARNING,
//     dataLoseAction
// })

// export const hideDataLoseWarning = () => (<Action<string>>{
//     type: COST_EDITOR_HIDE_LOSE_CHANGES_WARNING
// })

// export const loseChanges = () => asyncAction<CommonState>(
//     (dispatch, getState) => {
//         dispatch(hideDataLoseWarning());
//         dispatch(<Action<string>>{
//             type: COST_EDITOR_LOSE_CHANGES
//         })
        
//         const state = getState();

//         dispatch(state.pages.costEditor.dataLossInfo.action);
//     }
// )

// export const selectApplicationLosseDataCheck = (applicationId: string) => losseDataCheckHandlerAction(
//     (dispatch, { costBlocks: [ { costBlockId } ] }) => {
//         dispatch(selectApplication(applicationId));
//         dispatch(selectCostBlock(costBlockId));
//     }
// )