import { NamedId } from "../../Common/States/NamedId";
import { SelectList, MultiSelectList } from "../../Common/States/SelectList";

export interface EditItem extends NamedId {
    value: number
}

export interface CostElementInput extends NamedId {
    dependency: MultiSelectList<NamedId>
}

export interface CostBlockInput extends NamedId {
    selectedCountryId: string
    costElements: SelectList<CostElementInput>
    inputLevelSelectedId: {
        selectedId: string
        filterSelectedIds: string[]
    }
    editItems: EditItem[]
}
