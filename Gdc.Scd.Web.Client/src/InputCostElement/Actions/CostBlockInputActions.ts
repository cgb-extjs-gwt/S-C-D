import { Action } from "redux";
import { asyncAction, AsyncAction } from "../../Common/Actions/AsyncAction";
import * as service from "../Services/CostElementService";
import { NamedId } from "../../Common/States/NamedId";
import { CostElementInputState } from "../States/CostElementState";
import { PageCommonState } from "../../Layout/States/PageStates";
import { EditItem } from "../States/CostBlock";
import { losseDataCheckHandlerAction, losseDataCheckAction } from "../Helpers/CostElementHelper";

export const COST_BLOCK_INPUT_SELECT_COUNTRY = 'COST_BLOCK_INPUT.SELECT.COUNTRY';
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

export interface CostBlockInputAction extends Action<string>  {
    costBlockId: string 
}

export interface CountrySelectedAction extends CostBlockInputAction {
    countryId: string;
}

export interface CostElementAction extends CostBlockInputAction {
    costElementId: string
}

export interface FilterSelectionChangedAction extends CostBlockInputAction {
    filterItemId: string
    isSelected: boolean
}

export interface CostElementFilterSelectionChangedAction extends FilterSelectionChangedAction, CostElementAction {
}

export interface InputLevelAction extends CostBlockInputAction {
    inputLevelId: string
}

export interface InputLevelFilterSelectionChangedAction extends FilterSelectionChangedAction, InputLevelAction {
}

export interface CostlElementsFilterLoadedAction extends CostElementAction {
    filterItems: NamedId[]
}

export interface InputLevelFilterLoadedAction extends InputLevelAction {
    filterItems: NamedId[]
}

export interface EditItemsAction extends CostBlockInputAction {
    editItems: EditItem[]
}

export interface ItemEditedAction extends CostBlockInputAction {
    item: EditItem
}

export const selectCountry = (costBlockId: string, countryId: string) => (<CountrySelectedAction>{
    type:  COST_BLOCK_INPUT_SELECT_COUNTRY,
    costBlockId,
    countryId
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

export const selectInputLevel = (costBlockId: string, inputLevelId: string) => (<InputLevelAction>{
    type: COST_BLOCK_INPUT_SELECT_INPUT_LEVEL,
    costBlockId,
    inputLevelId
})

export const changeSelectionInputLevelFilter = (
    costBlockId: string, 
    inputLevelId: string, 
    filterItemId: string,
    isSelected: boolean
) => (<InputLevelFilterSelectionChangedAction>{
    type: COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER,
    costBlockId,
    inputLevelId,
    filterItemId,
    isSelected
})

export const resetInputLevelFilter = (costBlockId: string, inputLevelId: string) => (<InputLevelAction>{
    type: COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER,
    costBlockId,
    inputLevelId
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
    inputLevelId: string, 
    filterItems: NamedId[]
) => (<InputLevelFilterLoadedAction>{
    type: COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER,
    costBlockId,
    inputLevelId,
    filterItems
})

export const loadEditItems = (costBlockId: string, editItems: EditItem[]) => (<EditItemsAction>{
    type: COST_BLOCK_INPUT_LOAD_EDIT_ITEMS,
    costBlockId,
    editItems
})

export const clearEditItems = (costBlockId: string) => (<CostBlockInputAction>{
    type: COST_BLOCK_INPUT_CLEAR_EDIT_ITEMS,
    costBlockId
})

export const editItem = (costBlockId: string, item: EditItem) => (<ItemEditedAction>{
    type: COST_BLOCK_INPUT_EDIT_ITEM,
    costBlockId,
    item
})

export const saveEditItems = (costBlockId: string) => (<CostBlockInputAction>{
    type: COST_BLOCK_INPUT_SAVE_EDIT_ITEMS,
    costBlockId
})

export const applyFilters = (costBlockId: string) => (<CostBlockInputAction>{
    type: COST_BLOCK_INPUT_APPLY_FILTERS,
    costBlockId
})

export const getFilterItemsByCustomElementSelection = (costBlockId: string, costElementId: string) =>
    asyncAction<PageCommonState<CostElementInputState>>(
        (dispatch, { page }) => {
            dispatch(selectCostElement(costBlockId, costElementId));

            const costBlock = page.data.costBlocksInputs.find(item => item.costBlockId === costBlockId);
            const costElement = costBlock.costElement.list.find(item => item.costElementId === costElementId);
            
            if (!costElement.filter) {
                service.getCostElementFilterItems(costBlockId, costElementId).then(
                    filterItems => dispatch(loadCostElementFilter(costBlockId, costElementId, filterItems))
                )
            }
        }
    )

export const getFilterItemsByInputLevelSelection = (costBlockId: string, inputLevelId: string) =>
    asyncAction<PageCommonState<CostElementInputState>>(
        (dispatch, { page }) => {
            dispatch(selectInputLevel(costBlockId, inputLevelId));

            const costBlock = page.data.costBlocksInputs.find(item => item.costBlockId === costBlockId);
            const inputLevel = 
                costBlock.inputLevel.list && 
                costBlock.inputLevel.list.find(item => item.inputLevelId === inputLevelId);

            if (!inputLevel || !inputLevel.filter) {
                service.getLevelInputFilterItems(costBlockId, inputLevelId).then(
                    filterItems => dispatch(loadInputLevelFilter(costBlockId, inputLevelId, filterItems))
                )
            }
        }
    )

export const reloadFilterBySelectedCountry = (costBlockId: string, countryId: string) =>
    asyncAction<PageCommonState<CostElementInputState>>(
        (dispatch, { page }) => {
            dispatch(selectCountry(costBlockId, countryId));

            const costBlock = page.data.costBlocksInputs.find(item => item.costBlockId === costBlockId);
            const {
                costElement: { selectedItemId: costElementId },
                inputLevel: { selectedItemId: inputLevelId }
            } = costBlock;

            service.getCostElementFilterItems(costBlockId, costElementId).then(
                filterItems => dispatch(loadCostElementFilter(costBlockId, costElementId, filterItems))
            )

            service.getLevelInputFilterItems(costBlockId, inputLevelId).then(
                filterItems => dispatch(loadInputLevelFilter(costBlockId, inputLevelId, filterItems))
            )
        }
    )

const buildContext = (state: CostElementInputState) => {
    const { 
        selectedApplicationId: applicationId,  
        selectedScopeId: scopeId,
        selectedCostBlockId: costBlockId,
        costBlocksInputs
    } = state;

    const costBlock = costBlocksInputs.find(item => item.costBlockId === costBlockId); 

    const { 
        selectedCountryId: countryId,
        costElement,
        inputLevel                
    } = costBlock;

    let costElementFilterIds: string[] = null;
    let inputLevelFilterIds: string[] = null;

    if (costElement.selectedItemId != null && inputLevel.selectedItemId != null) {
        const selectedCostElement = 
            costElement.list.find(item => item.costElementId === costElement.selectedItemId);

        costElementFilterIds = selectedCostElement.filter 
            ? selectedCostElement.filter.map(item => item.id) 
            : [];

        const selectedInputLevel = 
            inputLevel.list.find(item => item.inputLevelId === inputLevel.selectedItemId);

        inputLevelFilterIds = selectedInputLevel.filter 
            ? selectedInputLevel.filter.map(item => item.id)
            : [];
    }

    return <service.Context>{
        applicationId,
        scopeId,
        costBlockId,
        countryId,
        costElementId: costElement.selectedItemId,
        inputLevelId: inputLevel.selectedItemId,
        costElementFilterIds,
        inputLevelFilterIds
    }
}

export const loadEditItemsByContext = () => 
    asyncAction<PageCommonState<CostElementInputState>>(
        (dispatch, { page }) => {
            const context = buildContext(page.data);

            if (context.costElementId != null && context.inputLevelId != null) {
                service.getEditItems(context).then(
                    editItems => dispatch(loadEditItems(context.costBlockId, editItems))
                )
            }
        }
    )

export const saveEditItemsToServer = (costBlockId: string) => 
    asyncAction<PageCommonState<CostElementInputState>>(
        (dispatch, { page }) => {
            const costBlock = 
                page.data.costBlocksInputs.find(item => item.costBlockId === costBlockId);

            const context = buildContext(page.data);

            service.saveEditItems(costBlock.edit.editedItems, context)
                   .then(
                       () => dispatch(saveEditItems(costBlockId))
                    )
        }
    )

export const selectCountryWithReloading = (costBlockId: string, countryId: string) => losseDataCheckHandlerAction(
    (dispatch, state) => {
        dispatch(reloadFilterBySelectedCountry(costBlockId, countryId));
        dispatch(loadEditItemsByContext());
    }
)

export const applyFiltersWithReloading = (costBlockId: string) => losseDataCheckHandlerAction(
    (dispatch, state) => {
        dispatch(applyFilters(costBlockId));
        dispatch(loadEditItemsByContext());
    }
)