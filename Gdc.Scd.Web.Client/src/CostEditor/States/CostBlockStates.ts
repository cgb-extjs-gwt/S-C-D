import { NamedId, SelectList } from "../../Common/States/CommonStates";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";


export interface EditItem extends NamedId<number> {
    value: number
    valueCount: number
    isApproved: boolean
    readonly: boolean
}

export interface CheckItem extends NamedId<number> {
    isChecked: boolean
}

export interface Filter {
    filter: CheckItem[]
}

export interface Region extends NamedId<number> {
    currency?: NamedId
    isReadOnly?: boolean
}

export interface InputLevelState extends Filter {
    inputLevelId: string
    isFilterLoaded: boolean
}

export interface CostElementState extends Filter {
    costElementId: string
    inputLevels: SelectList<InputLevelState>
    region: SelectList<Region, number>
    isDataLoaded: boolean
    referenceValues: NamedId<number>[]
}

export interface CostBlockEditState {
    editItemsUrl: string
    editedItems: EditItem[]
    saveErrors: BundleDetailGroup[]
}

export interface CostBlockState {
    costBlockId: string
    costElements: SelectList<CostElementState>
    edit: CostBlockEditState
}
