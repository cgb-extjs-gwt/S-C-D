import { NamedId } from "../../Common/States/NamedId";
import { SelectList, MultiSelectList } from "../../Common/States/SelectList";

export interface EditItem extends NamedId {
    value: number
}

export interface CheckItem extends NamedId {
    isChecked: boolean
}

export interface CostElementInput {
    costElementId: string
    filter: CheckItem[]
}

export interface InputLevelInput {
    inputLevelId: string
    filter: CheckItem[]
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
    }
}
