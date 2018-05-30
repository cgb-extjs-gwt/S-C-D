import { NamedId } from "../../Common/States/NamedId";
import { SelectList, MultiSelectList } from "../../Common/States/SelectList";

export interface EditItem extends NamedId {
    value: number
}

export interface CostElementInput {
    costElementId: string
    dependency: MultiSelectList<NamedId>
}

export interface CostBlockInputState {
    costBlockId: string
    isLoaded: boolean
    selectedCountryId: string
    costElements: SelectList<CostElementInput>
    visibleCostElementIds: string[]
    inputLevel: {
        selectedId: string
        filterSelectedIds: string[]
    }
    editItems: EditItem[]
}
