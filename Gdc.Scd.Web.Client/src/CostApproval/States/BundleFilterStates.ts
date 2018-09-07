import { NamedId, ElementWithParent } from "../../Common/States/CommonStates";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";

// export interface BundleFilter {
//     filter: BundleFilterStates,
//     applyFilter: BundleFilterStates
// }

export interface BundleFilterStates {
    selectedApplicationId: string
    selectedCostBlockIds: string[]
    selectedCostElementIds: ElementWithParent<string, string>[]
    startDate: Date
    endDate: Date
}


