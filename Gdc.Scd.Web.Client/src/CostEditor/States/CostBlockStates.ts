import { NamedId, SelectList } from "../../Common/States/CommonStates";


export interface EditItem extends NamedId {
    value: number
    valueCount: number
}

export interface CheckItem extends NamedId {
    isChecked: boolean
}

export interface Filter {
    filter: CheckItem[]
}

export interface CostElementState extends Filter {
    costElementId: string
    inputLevel: SelectList<InputLevelState>
    region: SelectList<NamedId>
}

export interface InputLevelState extends Filter {
    inputLevelId: string
}

export interface CostBlockState {
    costBlockId: string
    costElement: SelectList<CostElementState>
    visibleCostElementIds: string[]
    edit: {
        originalItems: EditItem[],
        editedItems: EditItem[]
        appliedFilter: {
            inputLevelItemIds: Set<string>
            costElementsItemIds: Set<string>
        }
    }
}
