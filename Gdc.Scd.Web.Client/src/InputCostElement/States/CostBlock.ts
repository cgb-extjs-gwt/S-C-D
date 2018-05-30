import { NamedId } from "../../Common/States/NamedId";
import { SelectList, MultiSelectList } from "../../Common/States/SelectList";

export interface EditItem extends NamedId {
    value: number
}

export interface CostElementInput {
    costElementId: string
    filter: MultiSelectList<NamedId>
}

export interface CostBlockInputState {
    costBlockId: string
    selectedCountryId: string
    costElement: SelectList<CostElementInput>
    visibleCostElementIds: string[]
    inputLevel: {
        selectedId: string
        filter: MultiSelectList<NamedId>
    }
    editItems: EditItem[]
}
