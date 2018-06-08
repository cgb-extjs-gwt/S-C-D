import { NamedId } from "../../Common/States/NamedId";
import { SelectList, MultiSelectList } from "../../Common/States/SelectList";

export interface EditItem extends NamedId {
    value: number
}

export interface CheckItem extends NamedId {
    isChecked: boolean
}

export interface Filter {
    filter: CheckItem[]
}

export interface CostElementInput extends Filter {
    costElementId: string
}

export interface InputLevelInput extends Filter {
    inputLevelId: string
}

export interface CostBlockInputState {
    costBlockId: string
    selectedCountryId: string
    costElement: SelectList<CostElementInput>
    visibleCostElementIds: string[]
    inputLevel: SelectList<InputLevelInput>
    edit: {
        originalItems: EditItem[],
        editedItems: EditItem[]
        appliedFilter: {
            inputLevelItemIds: Set<string>
            costElementsItemIds: Set<string>
        }
    }
}
