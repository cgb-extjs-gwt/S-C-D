import { NamedId } from "../../Common/States/CommonStates";

export interface CostElementId {
    costBlockId: string
    costElementId: string
}

export interface FilterState {
    selectedApplicationId: string
    selectedCostBlockIds: string[]
    selectedCostElementIds: CostElementId[]
    startDate: Date
    endDate: Date
}

export enum ApprovalBundleState {
    Saved = 0,
    Approving = 1,
    Approved = 2,
    Rejected = 3
}

export interface Bundle {
    id: number
    editDate: string
    editUser: NamedId
    editItemCount: number
    isDifferentValues: boolean
    application: NamedId
    regionInput: NamedId
    costBlock: NamedId
    costElement: NamedId
    inputLevel: NamedId
    qualityGateErrorExplanation: string
    state: ApprovalBundleState
}

export interface ApprovalCostElementsLayoutState<TFilter=FilterState> {
    bundles: Bundle[]
    filter: TFilter
}
