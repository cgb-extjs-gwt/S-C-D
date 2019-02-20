import { Action } from "redux";
import { EditItem } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { CommonState } from "../../Layout/States/AppStates";
import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";
import { CostElementData } from "../States/CostElementData";
import { CommonAction } from "../../Common/Actions/CommonActions";

export const COST_EDITOR_SELECT_COST_BLOCK = 'COST_EDITOR.SELECT.COST_BLOCK';
export const COST_BLOCK_INPUT_SELECT_REGIONS = 'COST_BLOCK_INPUT.SELECT.REGIONS';
export const COST_BLOCK_INPUT_SELECT_COST_ELEMENT = 'COST_BLOCK_INPUT.SELECT.COST_ELEMENT';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.COST_ELEMENT_FILTER';
export const COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER';
export const COST_BLOCK_INPUT_SELECT_INPUT_LEVEL = 'COST_BLOCK_INPUT.SELECT.INPUT_LEVEL';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.INPUT_LEVEL_FILTER';
export const COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.RESET.INPUT_LEVEL_FILTER';
export const COST_BLOCK_INPUT_LOAD_COST_ELEMENT_DATA = 'COST_BLOCK_INPUT.LOAD.COST_ELEMENT_DATA';
export const COST_BLOCK_INPUT_LOAD_DEPENDENCY_FILTER = 'COST_BLOCK_INPUT.LOAD.DEPENDENCY_FILTER';
export const COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.LOAD.INPUT_LEVEL_FILTER';
export const COST_BLOCK_INPUT_EDIT_ITEMS_URL_CHANGED = 'COST_BLOCK_INPUT.EDIT_ITEMS_URL.CHANGED';
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

export interface CostElementMetaAction extends CostElementAction {
    costBlockMeta: CostBlockMeta
}

export interface FilterSelectionChangedAction extends CostBlockAction {
    filterItemId: number
    isSelected: boolean
}

export interface RegionSelectedAction extends CostElementMetaAction {
    regionId: number;
}

export interface CostElementFilterSelectionChangedAction extends FilterSelectionChangedAction, CostElementMetaAction {
}

export interface InputLevelAction extends CostElementMetaAction {
    inputLevelId: string
}

export interface InputLevelFilterSelectionChangedAction extends FilterSelectionChangedAction, InputLevelAction {
}

export interface CostElementDataLoadedAction extends CostElementMetaAction {
    costElementData: CostElementData
    costBlockMeta: CostBlockMeta
}

export interface DependencyFilterLoadedAction extends CostElementAction {
    filterItems: NamedId<number>[]
}

export interface InputLevelFilterLoadedAction extends InputLevelAction {
    filterItems: NamedId<number>[]
}

export interface EditItemUrlChangedAction extends CostBlockAction {
    url: string
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

export const selectRegion = (applicationId: string, costBlockId: string, costElementId: string, regionId: number) => (<RegionSelectedAction>{
    type:  COST_BLOCK_INPUT_SELECT_REGIONS,
    applicationId,
    costBlockId,
    regionId,
    costElementId
})

export const selectCostElement = (applicationId: string, costElementId: string, costBlockMeta: CostBlockMeta) => (<CostElementMetaAction>{
    type:  COST_BLOCK_INPUT_SELECT_COST_ELEMENT,
    applicationId,
    costBlockId: costBlockMeta.id,
    costElementId,
    costBlockMeta
})

export const changeSelectionCostElementFilter = (
    applicationId: string,
    costBlockId: string, 
    costElementId: string, 
    filterItemId: number,
    isSelected: boolean
) => (<CostElementFilterSelectionChangedAction>{
    type: COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER,
    applicationId,
    costBlockId,
    costElementId,
    filterItemId,
    isSelected
})

export const resetCostElementFilter = (applicationId: string, costBlockId: string, costElementId: string) => (<CostElementMetaAction>{
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
    filterItemId: number,
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
    costElementId: string, 
    costBlockMeta: CostBlockMeta,
    costElementData: CostElementData
) => (<CostElementDataLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_COST_ELEMENT_DATA,
    applicationId,
    costBlockId: costBlockMeta.id, 
    costElementId,
    costElementData,
    costBlockMeta
})

export const loadDependencyFilter = (
    applicationId: string, 
    costBlockId: string, 
    costElementId: string, 
    dependencies: NamedId<number>[]
) => (<DependencyFilterLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_DEPENDENCY_FILTER,
    applicationId,
    costBlockId,
    costElementId,
    filterItems: dependencies
})

export const loadInputLevelFilter = (
    applicationId: string,
    costBlockId: string, 
    costElementId: string,
    inputLevelId: string, 
    filterItems: NamedId<number>[]
) => (<InputLevelFilterLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER,
    applicationId,
    costBlockId,
    costElementId,
    inputLevelId,
    filterItems
})

export const editItemsUrlChanged = (applicationId: string, costBlockId: string, url: string) => (<EditItemUrlChangedAction>{
    type: COST_BLOCK_INPUT_EDIT_ITEMS_URL_CHANGED,
    applicationId,
    costBlockId,
    url
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