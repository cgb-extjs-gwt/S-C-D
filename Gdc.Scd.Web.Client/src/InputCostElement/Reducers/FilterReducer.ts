import { CheckItem } from "../States/CostBlock";
import { Reducer } from "redux";
import { FilterSelectionChangedAction } from "../Actions/CostBlockInputActions";
import { mapIf } from "../../Common/Helpers/CommonHelpers";
import { NamedId } from "../../Common/States/NamedId";

export const changeSelecitonFilterItem: Reducer<CheckItem[], FilterSelectionChangedAction> = (state, action) => 
    mapIf(
        state, 
        filterItem => filterItem.id === action.filterItemId,
        filterItem => ({ ...filterItem, isChecked: action.isSelected }))

export const resetFilter = (state: CheckItem[]) => 
    state.map(filterItem => ({ 
        ...filterItem, 
        isChecked: false 
    }))

export const loadFilter = (filterItems: NamedId[]) => 
    filterItems.map(filterItem => (<CheckItem>{
        ...filterItem,  
        isChecked: false
    }))