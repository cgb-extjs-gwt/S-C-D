import { CostEditorState } from "../States/CostEditorStates";
import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { showDataLoseWarning } from "../Actions/CostEditorActions";
import { Dispatch } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { Context } from "../Services/CostEditorServices";
import { UsingInfo, CostBlockMeta, CostMetaData } from "../../Common/States/CostMetaStates";

const hasUnsavedChanges = (state: CostEditorState) => 
    !state.costBlocks.every(costBlock => !costBlock.edit.editedItems || costBlock.edit.editedItems.length === 0)

export const losseDataCheckHandlerAction = (
    handler: (dispatch: Dispatch, state: CostEditorState, mete: CostMetaData) => void
) => 
    asyncAction<CommonState>(
        (dispatch, getState, asyncAction) => {
            const { app: { appMetaData }, pages: { costEditor } } = getState();

            if (hasUnsavedChanges(costEditor)) {
                dispatch(showDataLoseWarning(asyncAction));
            } else {
                handler(dispatch, costEditor, appMetaData)
            }
        }
    )

// export const losseDataCheckAction = (action: Action<string>) => losseDataCheckHandlerAction(
//     dispatch => dispatch(action)
// )

export const buildCostEditorContext = (state: CostEditorState) => {
    const { 
        selectedApplicationId: applicationId,  
        selectedCostBlockId: costBlockId,
        costBlocks
    } = state;

    const costBlock = costBlocks.find(item => item.costBlockId === costBlockId); 

    const { 
        costElements,
    } = costBlock;

    let costElementFilterIds: string[] = null;
    let inputLevelFilterIds: string[] = null;
    let inputLevelId: string = null;
    let regionInputId: string = null;

    if (costElements.selectedItemId != null) {
        const selectedCostElement = 
            costElements.list.find(item => item.costElementId === costElements.selectedItemId);

        regionInputId = selectedCostElement.region && selectedCostElement.region.selectedItemId;
        inputLevelId = selectedCostElement.inputLevel.selectedItemId;
    }

    return <Context>{
        applicationId,
        costBlockId,
        regionInputId,
        costElementId: costElements.selectedItemId,
        inputLevelId,
        costElementFilterIds: Array.from(costBlock.edit.appliedFilter.costElementsItemIds),
        inputLevelFilterIds: Array.from(costBlock.edit.appliedFilter.inputLevelItemIds)
    }
}

export const filterCostEditorItems = <T extends UsingInfo>(items: T[]) => items.filter(item => item.isUsingCostEditor);
