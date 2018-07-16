import { Action } from "redux";
import { asyncAction, AsyncAction } from "../../Common/Actions/AsyncAction";
import * as service from "../Services/CostEditorServices";
import { CostEditorState } from "../States/CostEditorStates";
import { PageCommonState } from "../../Layout/States/PageStates";
import { EditItem } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { losseDataCheckHandlerAction } from "../Helpers/CostEditorHelpers";

export const COST_BLOCK_INPUT_LOAD_REGIONS = 'COST_BLOCK_INPUT.LOAD.REGIONS';
export const COST_BLOCK_INPUT_SELECT_REGIONS = 'COST_BLOCK_INPUT.SELECT.REGIONS';
export const COST_BLOCK_INPUT_SELECT_COST_ELEMENT = 'COST_BLOCK_INPUT.SELECT.COST_ELEMENT';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.COST_ELEMENT_FILTER'
export const COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER'
export const COST_BLOCK_INPUT_SELECT_INPUT_LEVEL = 'COST_BLOCK_INPUT.SELECT.INPUT_LEVEL';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.INPUT_LEVEL_FILTER'
export const COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.RESET.INPUT_LEVEL_FILTER'
export const COST_BLOCK_INPUT_LOAD_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT.LOAD.COST_ELEMENT_FILTER';
export const COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.LOAD.INPUT_LEVEL_FILTER';
export const COST_BLOCK_INPUT_LOAD_EDIT_ITEMS = 'COST_BLOCK_INPUT.LOAD.EDIT_ITEMS';
export const COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS = 'COST_BLOCK_INPUT.CLEAR.EDIT_ITEMS';
export const COST_BLOCK_INPUT_EDIT_ITEM = 'COST_BLOCK_INPUT.EDIT.ITEM';
export const COST_BLOCK_INPUT_SAVE_EDIT_ITEMS = 'COST_BLOCK_INPUT.SAVE.EDIT_ITEMS';
export const COST_BLOCK_INPUT_APPLY_FILTERS = 'COST_BLOCK_INPUT.APPLY.FILTERS';

export interface CostBlockAction extends Action<string>  {
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

export interface RegionLoadedAction extends CostElementAction {
    regions: NamedId[]
}

export interface CostlElementsFilterLoadedAction extends CostElementAction {
    filterItems: NamedId[]
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

export const selectRegion = (costBlockId: string, costElementId: string, regionId: string) => (<RegionSelectedAction>{
    type:  COST_BLOCK_INPUT_SELECT_REGIONS,
    costBlockId,
    regionId,
    costElementId
})

export const selectCostElement = (costBlockId: string, costElementId: string) => (<CostElementAction>{
    type:  COST_BLOCK_INPUT_SELECT_COST_ELEMENT,
    costBlockId,
    costElementId
})

export const changeSelectionCostElementFilter = (
    costBlockId: string, 
    costElementId: string, 
    filterItemId: string,
    isSelected: boolean
) => (<CostElementFilterSelectionChangedAction>{
    type: COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER,
    costBlockId,
    costElementId,
    filterItemId,
    isSelected
})

export const resetCostElementFilter = (costBlockId: string, costElementId: string) => (<CostElementAction>{
    type: COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER,
    costBlockId,
    costElementId
})

export const selectInputLevel = (costBlockId: string, costElementId: string,  inputLevelId: string) => (<InputLevelAction>{
    type: COST_BLOCK_INPUT_SELECT_INPUT_LEVEL,
    costBlockId,
    costElementId,
    inputLevelId
})

export const changeSelectionInputLevelFilter = (
    costBlockId: string, 
    costElementId: string,
    inputLevelId: string, 
    filterItemId: string,
    isSelected: boolean
) => (<InputLevelFilterSelectionChangedAction>{
    type: COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER,
    costBlockId,
    costElementId,
    inputLevelId,
    filterItemId,
    isSelected
})

export const resetInputLevelFilter = (costBlockId: string, inputLevelId: string) => (<InputLevelAction>{
    type: COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER,
    costBlockId,
    inputLevelId
}) 

export const loadRegions = (costBlockId: string, costElementId: string, regions: NamedId[]) => (<RegionLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_REGIONS,
    costBlockId,
    costElementId,
    regions
})

export const loadCostElementFilter = (
    costBlockId: string, 
    costElementId: string, 
    filterItems: NamedId[]
) => (<CostlElementsFilterLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_COST_ELEMENT_FILTER,
    costBlockId,
    costElementId,
    filterItems
})

export const loadInputLevelFilter = (
    costBlockId: string, 
    costElementId: string,
    inputLevelId: string, 
    filterItems: NamedId[]
) => (<InputLevelFilterLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER,
    costBlockId,
    costElementId,
    inputLevelId,
    filterItems
})

export const loadEditItems = (costBlockId: string, editItems: EditItem[]) => (<EditItemsAction>{
    type: COST_BLOCK_INPUT_LOAD_EDIT_ITEMS,
    costBlockId,
    editItems
})

export const clearEditItems = (costBlockId: string) => (<CostBlockAction>{
    type: COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS,
    costBlockId
})

export const editItem = (costBlockId: string, item: EditItem) => (<ItemEditedAction>{
    type: COST_BLOCK_INPUT_EDIT_ITEM,
    costBlockId,
    item
})

export const saveEditItems = (costBlockId: string) => (<CostBlockAction>{
    type: COST_BLOCK_INPUT_SAVE_EDIT_ITEMS,
    costBlockId
})

export const applyFilters = (costBlockId: string) => (<CostBlockAction>{
    type: COST_BLOCK_INPUT_APPLY_FILTERS,
    costBlockId
})

const buildContext = (state: CostEditorState) => {
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

        if (selectedCostElement.inputLevel.selectedItemId != null)
        {
            inputLevelId = selectedCostElement.inputLevel.selectedItemId;

            const selectedInputLevel = 
                selectedCostElement.inputLevel.list.find(item => item.inputLevelId === selectedCostElement.inputLevel.selectedItemId);

            costElementFilterIds = selectedCostElement.filter 
                ? selectedCostElement.filter.filter(item => item.isChecked).map(item => item.id) 
                : [];

            inputLevelFilterIds = selectedInputLevel.filter 
                ? selectedInputLevel.filter.filter(item => item.isChecked).map(item => item.id)
                : [];
        }
    }

    return <service.Context>{
        applicationId,
        costBlockId,
        regionInputId,
        costElementId: costElement.selectedItemId,
        inputLevelId,
        costElementFilterIds,
        inputLevelFilterIds
    }
}

export const getDataByCostElementSelection = (costBlockId: string, costElementId: string) =>
    asyncAction<PageCommonState<CostEditorState>>(
        (dispatch, getState) => {
            dispatch(selectCostElement(costBlockId, costElementId));

            const { page } = getState();
            const { data: state } = page;
            const costElementMeta = state.costBlockMetas.get(costBlockId).costElements.find(item => item.id === costElementId);
            const context = buildContext(state);
            const costBlock = state.costBlocks.find(item => item.costBlockId === costBlockId);
            const costElement = costBlock.costElement.list.find(item => item.costElementId === costElementId);

            if (costElementMeta.dependency && !costElement.filter) {
                service.getCostElementFilterItems(context).then(
                    filterItems => dispatch(loadCostElementFilter(costBlockId, costElementId, filterItems))
                )
            }

            if (costElementMeta.regionInput && !costElement.region) {
                service.getRegions(context).then(regions => dispatch(loadRegions(costBlockId, costElementId, regions)));
            }
        }
    )

export const getFilterItemsByInputLevelSelection = (costBlockId: string, costElementId: string, inputLevelId: string) =>
    asyncAction<PageCommonState<CostEditorState>>(
        (dispatch, getState) => {
            dispatch(selectInputLevel(costBlockId, costElementId, inputLevelId));

            const { page } = getState();
            const { data: state } = page;

            const costBlockMeta = state.costBlockMetas.get(costBlockId);
            const costElementMeta = costBlockMeta.costElements.find(item => item.id === costElementId);
            const inputLevelMeta = costElementMeta.inputLevels.find(item => item.id === inputLevelId);

            if (inputLevelMeta.isFilterLoading) {
                const costBlock = state.costBlocks.find(item => item.costBlockId === costBlockId);
                const costElement = costBlock.costElement.list.find(item => item.costElementId === costBlock.costElement.selectedItemId);
                const inputLevel = costElement.inputLevel.list.find(item => item.inputLevelId === inputLevelId);
                
                if (!inputLevel || !inputLevel.filter)
                {
                    const context = buildContext(state);

                    service.getLevelInputFilterItems(context).then(
                        filterItems => dispatch(loadInputLevelFilter(costBlockId, costElementId, inputLevelId, filterItems))
                    )
                }
            }
        }
    )

export const reloadFilterBySelectedRegion = (costBlockId: string, regionId: string) =>
    asyncAction<PageCommonState<CostEditorState>>(
        (dispatch, getState) => {
            if (regionId) {
                const { page } = getState();
                const costBlock = page.data.costBlocks.find(item => item.costBlockId === costBlockId);

                const {
                    costElement: { selectedItemId: costElementId },
                } = costBlock;

                const costElement = costBlock.costElement.list.find(item => item.costElementId === costElementId);

                if (costElement.region && costElement.region.selectedItemId !== regionId) {
                    dispatch(selectRegion(costBlockId, costElementId, regionId));
                    dispatch(getDataByCostElementSelection(costBlockId, costElementId));
                    
                    if (costElement.inputLevel.selectedItemId) {
                        dispatch(getFilterItemsByInputLevelSelection(costBlockId, costElementId, costElement.inputLevel.selectedItemId));
                    }
                }
            }
        }
    )

export const loadEditItemsByContext = () => 
    asyncAction<PageCommonState<CostEditorState>>(
        (dispatch, getState) => {
            const { page } = getState();
            const context = buildContext(page.data);

            if (context.costElementId != null && context.inputLevelId != null) {
                service.getEditItems(context).then(
                    editItems => dispatch(loadEditItems(context.costBlockId, editItems))
                )
            }
        }
    )

export const saveEditItemsToServer = (costBlockId: string) => 
    asyncAction<PageCommonState<CostEditorState>>(
        (dispatch, getState) => {
            const { page } = getState();
            const costBlock = 
                page.data.costBlocks.find(item => item.costBlockId === costBlockId);

            const context = buildContext(page.data);

            service.saveEditItems(costBlock.edit.editedItems, context)
                   .then(
                       () => dispatch(saveEditItems(costBlockId))
                    )
        }
    )

export const selectRegionWithReloading = (costBlockId: string, regionId: string) => losseDataCheckHandlerAction(
    (dispatch, state) => {
        dispatch(reloadFilterBySelectedRegion(costBlockId, regionId));
        dispatch(loadEditItemsByContext());
    }
)

export const applyFiltersWithReloading = (costBlockId: string) => losseDataCheckHandlerAction(
    (dispatch, state) => {
        dispatch(applyFilters(costBlockId));
        dispatch(loadEditItemsByContext());
    }
)