import { Action } from "redux";
import { asyncAction, AsyncAction } from "../../Common/Actions/AsyncAction";
import * as service from "../Services/CostEditorServices";
import { CostEditorState } from "../States/CostEditorStates";
import { EditItem, CostElementData } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { buildCostEditorContext, findCostElementByState, findInputeLevelByState, findCostBlockByState } from "../Helpers/CostEditorHelpers";
import { CommonState } from "../../Layout/States/AppStates";
import { ApprovalOption } from "../Services/CostEditorServices";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";
import { findMeta } from "../../Common/Helpers/MetaHelper";

export const COST_EDITOR_SELECT_COST_BLOCK = 'COST_EDITOR.SELECT.COST_BLOCK';
export const COST_BLOCK_INPUT_SELECT_REGIONS = 'COST_BLOCK_INPUT.SELECT.REGIONS';
export const COST_BLOCK_INPUT_SELECT_COST_ELEMENT = 'COST_BLOCK_INPUT.SELECT.COST_ELEMENT';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.COST_ELEMENT_FILTER'
export const COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER'
export const COST_BLOCK_INPUT_SELECT_INPUT_LEVEL = 'COST_BLOCK_INPUT.SELECT.INPUT_LEVEL';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.INPUT_LEVEL_FILTER'
export const COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.RESET.INPUT_LEVEL_FILTER'
export const COST_BLOCK_INPUT_LOAD_COST_ELEMENT_DATA = 'COST_BLOCK_INPUT.LOAD.COST_ELEMENT_DATA';
export const COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.LOAD.INPUT_LEVEL_FILTER';
export const COST_BLOCK_INPUT_LOAD_EDIT_ITEMS = 'COST_BLOCK_INPUT.LOAD.EDIT_ITEMS';
export const COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS = 'COST_BLOCK_INPUT.CLEAR.EDIT_ITEMS';
export const COST_BLOCK_INPUT_EDIT_ITEM = 'COST_BLOCK_INPUT.EDIT.ITEM';
export const COST_BLOCK_INPUT_SAVE_EDIT_ITEMS = 'COST_BLOCK_INPUT.SAVE.EDIT_ITEMS';
export const COST_BLOCK_INPUT_APPLY_FILTERS = 'COST_BLOCK_INPUT.APPLY.FILTERS';
export const COST_BLOCK_INPUT_RESET_ERRORS = 'COST_BLOCK_INPUT.RESET.ERRORS';

export interface CostBlockAction extends Action<string>  {
    applicationId: string
    costBlockId: string 
}

export interface CostElementAction extends CostBlockAction {
    costElementId: string
}

export interface FilterSelectionChangedAction extends CostBlockAction {
    filterItemId: string
    isSelected: boolean
}

export interface RegionSelectedAction extends CostElementAction {
    regionId: string;
}

export interface CostElementFilterSelectionChangedAction extends FilterSelectionChangedAction, CostElementAction {
}

export interface InputLevelAction extends CostElementAction {
    inputLevelId: string
}

export interface InputLevelFilterSelectionChangedAction extends FilterSelectionChangedAction, InputLevelAction {
}

export interface CostElementDataLoadedAction extends CostElementAction {
    costElementData: CostElementData
}

export interface InputLevelFilterLoadedAction extends InputLevelAction {
    filterItems: NamedId[]
}

export interface EditItemsAction extends CostBlockAction {
    editItems: EditItem[]
}

export interface ItemEditedAction extends CostBlockAction {
    item: EditItem
}

export interface SaveEditItemsAction extends CostBlockAction {
    qualityGateResult: QualityGateResult
}

export const selectCostBlock = (applicationId: string, costBlockId: string) => (<CostBlockAction>{
    type: COST_EDITOR_SELECT_COST_BLOCK,
    applicationId,
    costBlockId
});

export const selectRegion = (applicationId: string, costBlockId: string, costElementId: string, regionId: string) => (<RegionSelectedAction>{
    type:  COST_BLOCK_INPUT_SELECT_REGIONS,
    applicationId,
    costBlockId,
    regionId,
    costElementId
})

export const selectCostElement = (applicationId: string, costBlockId: string, costElementId: string) => (<CostElementAction>{
    type:  COST_BLOCK_INPUT_SELECT_COST_ELEMENT,
    applicationId,
    costBlockId,
    costElementId
})

export const changeSelectionCostElementFilter = (
    applicationId: string,
    costBlockId: string, 
    costElementId: string, 
    filterItemId: string,
    isSelected: boolean
) => (<CostElementFilterSelectionChangedAction>{
    type: COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER,
    applicationId,
    costBlockId,
    costElementId,
    filterItemId,
    isSelected
})

export const resetCostElementFilter = (applicationId: string, costBlockId: string, costElementId: string) => (<CostElementAction>{
    type: COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER,
    applicationId,
    costBlockId,
    costElementId
})

export const selectInputLevel = (applicationId: string, costBlockId: string, costElementId: string,  inputLevelId: string) => (<InputLevelAction>{
    type: COST_BLOCK_INPUT_SELECT_INPUT_LEVEL,
    applicationId,
    costBlockId,
    costElementId,
    inputLevelId
})

export const changeSelectionInputLevelFilter = (
    applicationId: string,
    costBlockId: string, 
    costElementId: string,
    inputLevelId: string, 
    filterItemId: string,
    isSelected: boolean
) => (<InputLevelFilterSelectionChangedAction>{
    type: COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER,
    applicationId,
    costBlockId,
    costElementId,
    inputLevelId,
    filterItemId,
    isSelected
})

export const resetInputLevelFilter = (applicationId: string, costBlockId: string, costElementId: string, inputLevelId: string) => (<InputLevelAction>{
    type: COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER,
    applicationId,
    costBlockId,
    costElementId,
    inputLevelId
}) 

export const loadCostElementData = (
    applicationId: string,
    costBlockId: string, 
    costElementId: string, 
    costElementData: CostElementData
) => (<CostElementDataLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_COST_ELEMENT_DATA,
    applicationId,
    costBlockId, 
    costElementId,
    costElementData
})

export const loadInputLevelFilter = (
    applicationId: string,
    costBlockId: string, 
    costElementId: string,
    inputLevelId: string, 
    filterItems: NamedId[]
) => (<InputLevelFilterLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER,
    applicationId,
    costBlockId,
    costElementId,
    inputLevelId,
    filterItems
})

export const loadEditItems = (applicationId: string, costBlockId: string, editItems: EditItem[]) => (<EditItemsAction>{
    type: COST_BLOCK_INPUT_LOAD_EDIT_ITEMS,
    applicationId,
    costBlockId,
    editItems
})

export const clearEditItems = (applicationId: string, costBlockId: string) => (<CostBlockAction>{
    type: COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS,
    applicationId,
    costBlockId
})

export const editItem = (applicationId: string, costBlockId: string, item: EditItem) => (<ItemEditedAction>{
    type: COST_BLOCK_INPUT_EDIT_ITEM,
    applicationId,
    costBlockId,
    item
})

export const saveEditItems = (applicationId: string, costBlockId: string, qualityGateResult: QualityGateResult) => (<SaveEditItemsAction>{
    type: COST_BLOCK_INPUT_SAVE_EDIT_ITEMS,
    applicationId,
    costBlockId,
    qualityGateResult
})

export const applyFilters = (applicationId: string, costBlockId: string) => (<CostBlockAction>{
    type: COST_BLOCK_INPUT_APPLY_FILTERS,
    applicationId,
    costBlockId
})

export const resetErrors = (applicationId: string, costBlockId: string) => (<CostBlockAction>{
    type: COST_BLOCK_INPUT_RESET_ERRORS,
    applicationId,
    costBlockId
})

export const getDataByCostElementSelection = (applicationId: string, costBlockId: string, costElementId: string) =>
    asyncAction<CommonState>(
        (dispatch, getState) => {
            dispatch(selectCostElement(applicationId, costBlockId, costElementId));

            const state = getState().pages.costEditor
            const context = buildCostEditorContext(state);
            const costElement = findCostElementByState(state, applicationId, costBlockId, costElementId);

            if (costElement.isDataLoaded) {
                handleRequest(
                    service.getCostElementData(context).then(
                        data => dispatch(loadCostElementData(applicationId, costBlockId, costElementId, data))
                    )
                )
            }

            if (costElement.inputLevels.selectedItemId == null){
                dispatch(
                    getFilterItemsByInputLevelSelection(
                        applicationId,
                        costBlockId, 
                        costElementId, 
                        costElement.
                        inputLevels.list[0].inputLevelId));
            }
        }
    )

export const getFilterItemsByInputLevelSelection = (applicationId: string, costBlockId: string, costElementId: string, inputLevelId: string) =>
    asyncAction<CommonState>(
        (dispatch, getState) => {
            dispatch(selectInputLevel(applicationId, costBlockId, costElementId, inputLevelId));

            const state = getState();
            const costEditor = state.pages.costEditor;
            const costBlockMeta = findMeta(state.app.appMetaData.costBlocks, costBlockId);
            const costElementMeta = findMeta(costBlockMeta.costElements, costElementId);
            const inputLevelMeta = findMeta(costElementMeta.inputLevels, inputLevelId);

            if (inputLevelMeta.hasFilter) {
                const inputLevel = findInputeLevelByState(costEditor, applicationId, costBlockId, costElementId, inputLevelId)
                
                if (!inputLevel || !inputLevel.filter)
                {
                    const context = buildCostEditorContext(costEditor);
                    
                    handleRequest(
                        service.getLevelInputFilterItems(context).then(
                            filterItems => dispatch(loadInputLevelFilter(applicationId, costBlockId, costElementId, inputLevelId, filterItems))
                        )
                    )
                }
            }
        }
    )

export const loadEditItemsByContext = () => 
    asyncAction<CommonState>(
        (dispatch, getState) => {
            const { app: { appMetaData }, pages: { costEditor } } = getState();
            const context = buildCostEditorContext(costEditor);
            const costBlockMeta = findMeta(appMetaData.costBlocks, context.costBlockId);
            const { regionInput } = findMeta(costBlockMeta.costElements, context.costElementId);

            if (context.costElementId != null && context.inputLevelId != null && (!regionInput || context.regionInputId)) {
                handleRequest(
                    service.getEditItems(context).then(
                        editItems => dispatch(loadEditItems(context.applicationId, context.costBlockId, editItems))
                    )
                )
            }
        }
    )

export const saveEditItemsToServer = (applicationId: string, costBlockId: string, approvalOption: ApprovalOption) => 
    asyncAction<CommonState>(
        (dispatch, getState) => {
            const state = getState().pages.costEditor
            const costBlock = findCostBlockByState(state, applicationId, costBlockId);
            const context = buildCostEditorContext(state);

            handleRequest(
                service.saveEditItems(costBlock.edit.editedItems, context, approvalOption)
                       .then(
                            qualityGateResult => dispatch(saveEditItems(applicationId, costBlockId, qualityGateResult))
                       )
            )
        }
    )

export const selectRegionWithReloading = (applicationId: string, costBlockId: string, regionId: string) => asyncAction<CommonState>(
    (dispatch, getState) => {
        const state = getState();
        const costBlock = findCostBlockByState(state.pages.costEditor, applicationId, costBlockId);

        dispatch(selectRegion(applicationId, costBlockId, costBlock.costElements.selectedItemId, regionId));
        dispatch(loadEditItemsByContext());
    }
)

export const applyFiltersWithReloading = (applicationId: string, costBlockId: string) => asyncAction<CommonState>(
     dispatch => {
        dispatch(applyFilters(applicationId, costBlockId));
        dispatch(loadEditItemsByContext());
    }
)
