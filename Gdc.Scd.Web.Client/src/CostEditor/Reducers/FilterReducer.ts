import { CheckItem } from "../States/CostBlockStates";
import { Reducer } from "redux";
import { FilterSelectionChangedAction } from "../Actions/CostBlockActions";
import { mapIf } from "../../Common/Helpers/CommonHelpers";
import { NamedId } from "../../Common/States/CommonStates";

export const changeSelecitonFilterItem: Reducer<CheckItem[], FilterSelectionChangedAction> = (state, action) => 
    mapIf(
        state, 
        filterItem => filterItem.id === action.filterItemId,
        filterItem => ({ ...filterItem, isChecked: action.isSelected }))

export const resetFilter = (state: CheckItem[]) => 
    state.map(filterItem => ({ 
        ...filterItem, 
        isChecked: true 
    }))

export const loadFilter = (filterItems: NamedId<number>[]) => 
    filterItems && filterItems.map(filterItem => (<CheckItem>{
        ...filterItem,  
        isChecked: true
    }))