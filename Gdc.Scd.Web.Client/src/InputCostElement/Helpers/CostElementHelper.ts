import { CostElementInputState } from "../States/CostElementState";
import { CostBlockInputState } from "../States/CostBlock";
import { Action } from "redux";
import { PageCommonState } from "../../Layout/States/PageStates";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { showDataLoseWarning } from "../Actions/InputCostElementActions";
import { Dispatch } from "react-redux";

const hasUnsavedChanges = (state: CostElementInputState) => 
    !state.costBlocksInputs.every(costBlock => !costBlock.edit.editedItems || costBlock.edit.editedItems.length === 0)

// const clearChanges = (costBlocks: CostBlockInputState[]) => 
//     costBlocks.map(costBlock => (<CostBlockInputState>{
//         ...costBlock,
//         edit: {
//             ...costBlock.edit,
//             editedItems: []
//         }
//     }))

// export const buildLoseDataChecker = <TAction extends Action<string>>(
//     fn: (state: CostElementInputState, action: TAction) => CostElementInputState,
// ) => 
//     (state: CostElementInputState, action: Action<string>): CostElementInputState  => {
//         let result: CostElementInputState;

//         const { isWarningDisplayed } = state.dataLossInfo;

//         if (isWarningDisplayed) {
//             result = state;
//         } 
//         // else if (isLoseChanges) {
//         //     result = { 
//         //         ...fn(state, <TAction>action),
//         //         costBlocksInputs: clearChanges(state.costBlocksInputs),
//         //         dataLossInfo: {
//         //             ...state.dataLossInfo,
//         //             isLoseChanges: false
//         //         }
//         //     };
//         // } 
//         else if(hasUnsavedChanges(state)){
//             result = {
//                 ...state,
//                 dataLossInfo: {
//                     ...state.dataLossInfo,
//                     isWarningDisplayed: true,
//                     action
//                 }
//             }
//         } else {
//             result = fn(state, <TAction>action);
//         }

//         return result;
//     }

export const losseDataCheckHandlerAction = (
    handler: (dispatch: Dispatch, state: PageCommonState<CostElementInputState>) => void
) => 
    asyncAction<PageCommonState<CostElementInputState>>(
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