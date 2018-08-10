import { CostEditorState } from "../States/CostEditorStates";
import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { showDataLoseWarning } from "../Actions/CostEditorActions";
import { Dispatch } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { Context } from "../Services/CostEditorServices";

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

export const buildCostEditorContext = (state: CostEditorState) => {
    const { 
        selectedApplicationId: applicationId,  
        selectedCostBlockId: costBlockId,
        costBlocks
    } = state;

    const costBlock = costBlocks.find(item => item.costBlockId === costBlockId); 

    const { 
        costElement,
    } = costBlock;

    let costElementFilterIds: string[] = null;
    let inputLevelFilterIds: string[] = null;
    let inputLevelId: string = null;
    let regionInputId: string = null;

    if (costElement.selectedItemId != null) {
        const selectedCostElement = 
            costElement.list.find(item => item.costElementId === costElement.selectedItemId);

        regionInputId = selectedCostElement.region && selectedCostElement.region.selectedItemId;
        inputLevelId = selectedCostElement.inputLevel.selectedItemId;
    }

    return <Context>{
        applicationId,
        costBlockId,
        regionInputId,
        costElementId: costElement.selectedItemId,
        inputLevelId,
        costElementFilterIds: Array.from(costBlock.edit.appliedFilter.costElementsItemIds),
        inputLevelFilterIds: Array.from(costBlock.edit.appliedFilter.inputLevelItemIds)
    }
}