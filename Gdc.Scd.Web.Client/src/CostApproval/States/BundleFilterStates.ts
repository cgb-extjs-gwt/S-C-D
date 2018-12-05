import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";

export interface CostElementId {
    costBlockId: string
    costElementId: string
}

export interface BundleFilterStates {
    selectedApplicationId: string
    selectedCostBlockIds: string[]
    selectedCostElementIds: CostElementId[]
    startDate: Date
    endDate: Date
}


