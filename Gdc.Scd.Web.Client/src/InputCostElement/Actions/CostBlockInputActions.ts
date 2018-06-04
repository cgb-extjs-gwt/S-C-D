import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { getCostElementFilterItems, getLevelInputFilterItems } from "../Services/CostElementService";
import { NamedId } from "../../Common/States/NamedId";
import { CostElementInputState } from "../States/CostElementState";
import { PageCommonState } from "../../Layout/States/PageStates";

export const COST_BLOCK_INPUT_SELECT_COUNTRY = 'COST_BLOCK_INPUT.SELECT.COUNTRY';
export const COST_BLOCK_INPUT_SELECT_COST_ELEMENT = 'COST_BLOCK_INPUT.SELECT.COST_ELEMENT';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.COST_ELEMENT_FILTER'
export const COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT_RESET_COST_ELEMENT_FILTER'
export const COST_BLOCK_INPUT_SELECT_INPUT_LEVEL = 'COST_BLOCK_INPUT.SELECT.INPUT_LEVEL';
export const COST_BLOCK_INPUT_SELECTION_CHANGE_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.SELECTION_CHANGE.INPUT_LEVEL_FILTER'
export const COST_BLOCK_INPUT_RESET_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.RESET.INPUT_LEVEL_FILTER'
export const COST_BLOCK_INPUT_LOAD_COST_ELEMENT_FILTER = 'COST_BLOCK_INPUT.LOAD.COST_ELEMENT_FILTER';
export const COST_BLOCK_INPUT_LOAD_INPUT_LEVEL_FILTER = 'COST_BLOCK_INPUT.LOAD.INPUT_LEVEL_FILTER';

export interface CostBlockInputAction extends Action<string>  {
    costBlockId: string 
}

export interface CountrySelectedAction extends CostBlockInputAction {
    countryId: string
}

// export interface CostElementId {
//     costElementId: string
// }

export interface CostElementAction extends CostBlockInputAction {
    costElementId: string
}

export interface FilterSelectionChangedAction extends CostBlockInputAction {
    filterItemId: string
    isSelected: boolean
}

export interface CostElementFilterSelectionChangedAction extends FilterSelectionChangedAction, CostElementAction {
}

// export interface CostElementFilterResetedAction extends CostElementAction {
// }

// export interface InputLevelId {
//     inputLevelId: string
// }

export interface InputLevelAction extends CostBlockInputAction {
    inputLevelId: string
}

export interface InputLevelFilterSelectionChangedAction extends FilterSelectionChangedAction, InputLevelAction {
}

// export interface InputLevelFilterResetedAction extends InputLevelAction {
// }

export interface CostlElementsFilterLoadedAction extends CostElementAction {
    filterItems: NamedId[]
}

export interface InputLevelFilterLoadedAction extends InputLevelAction {
    filterItems: NamedId[]
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

export const getFilterItemsByCustomElementSelection = (costBlockId: string, costElementId: string) =>
    asyncAction<PageCommonState<CostElementInputState>>(
        (dispatch, { page }) => {
            dispatch(selectCostElement(costBlockId, costElementId));

            const costBlock = page.data.costBlocksInputs.find(item => item.costBlockId === costBlockId);
            const costElement = costBlock.costElement.list.find(item => item.costElementId === costElementId);
            
            if (!costElement.filter) {
                getCostElementFilterItems(costBlockId, costElementId).then(
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

            if (!costBlock.inputLevel.filter) {
                getLevelInputFilterItems(costBlockId, inputLevelId).then(
                    filterItems => dispatch(loadInputLevelFilter(costBlockId, inputLevelId, filterItems))
                )
            }
        }
    )