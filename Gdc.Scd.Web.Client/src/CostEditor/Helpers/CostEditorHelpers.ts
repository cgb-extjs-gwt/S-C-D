import { CostEditorState } from "../States/CostEditorStates";
import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { Dispatch } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { Context } from "../Services/CostEditorServices";
import { UsingInfo, CostBlockMeta, CostMetaData } from "../../Common/States/CostMetaStates";
import { SelectList } from "../../Common/States/CommonStates";
import { CostBlockState, CostElementState, InputLevelState } from "../States/CostBlockStates";

export const findApplication = (state: CostEditorState, applicationId = state.applications.selectedItemId) => 
    state.applications.list.find(app => app.id == applicationId)

export const findCostBlock = (costBlocks: SelectList<CostBlockState>, costBlockId = costBlocks.selectedItemId) => 
    costBlocks.list.find(costBlock => costBlock.costBlockId == costBlockId)

export const findCostElement = (costElements: SelectList<CostElementState>, costElementId = costElements.selectedItemId) => 
    costElements.list.find(costElement => costElement.costElementId == costElementId)

export const findInputLevel = (inputLevels: SelectList<InputLevelState>, inputLevelId = inputLevels.selectedItemId) =>
    inputLevels.list.find(inputLevel => inputLevel.inputLevelId == inputLevelId)

export const findCostBlockByState = (state: CostEditorState, applicationId?: string, costBlockId?: string) => {
    const { costBlocks } = findApplication(state, applicationId);

    return findCostBlock(costBlocks, costBlockId)
}

export const findCostElementByState = (state: CostEditorState, applicationId?: string, costBlockId?: string, costElementId?: string) => {
    const { costElements } = findCostBlockByState(state, applicationId, costBlockId);

    if (costElementId == null) {
        costElementId = costElements.selectedItemId;
    }

    return findCostElement(costElements, costElementId);
}

export const findInputeLevelByState = (state: CostEditorState, applicationId?: string, costBlockId?: string, costElementId?: string, inputeLevelId?: string) => {
    const { inputLevels } = findCostElementByState(state, applicationId, costBlockId, costElementId);

    return findInputLevel(inputLevels, inputeLevelId);
}

export const buildCostEditorContext = (state: CostEditorState) => {
    // const { 
    //     selectedApplicationId: applicationId,  
    //     selectedCostBlockId: costBlockId,
    //     costBlocks
    // } = state;

    // const costBlock = costBlocks.find(item => item.costBlockId === costBlockId); 

    // const { 
    //     costElements,
    // } = costBlock;

    const { id: applicationId, costBlocks } = findApplication(state);
    const costBlock = findCostBlock(costBlocks);
    const { costBlockId, costElements } = costBlock;

    let costElementFilterIds: string[] = null;
    let inputLevelFilterIds: string[] = null;
    let inputLevelId: string = null;
    let regionInputId: string = null;

    if (costElements.selectedItemId != null) {
        const selectedCostElement = 
            costElements.list.find(item => item.costElementId === costElements.selectedItemId);

        regionInputId = selectedCostElement.region && selectedCostElement.region.selectedItemId;
        inputLevelId = selectedCostElement.inputLevels.selectedItemId;
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

// const hasUnsavedChanges = (state: CostEditorState) => 
//     !state.costBlocks.every(costBlock => !costBlock.edit.editedItems || costBlock.edit.editedItems.length === 0)

// const hasUnsavedChanges = (state: CostEditorState) => {
//     const { costBlocks } = findApplication(state);

//     return !costBlocks.list.every(costBlock => !costBlock.edit.editedItems || costBlock.edit.editedItems.length === 0);
// }

// export const losseDataCheckHandlerAction = (
//     handler: (dispatch: Dispatch, state: CostEditorState, meta: CostMetaData) => void
// ) => 
//     asyncAction<CommonState>(
//         (dispatch, getState, asyncAction) => {
//             const { app: { appMetaData }, pages: { costEditor } } = getState();

//             if (hasUnsavedChanges(costEditor)) {
//                 dispatch(showDataLoseWarning(asyncAction));
//             } else {
//                 handler(dispatch, costEditor, appMetaData)
//             }
//         }
//     )

// export const losseDataCheckAction = (action: Action<string>) => losseDataCheckHandlerAction(
//     dispatch => dispatch(action)
// )