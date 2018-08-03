import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta } from "../../Common/States/CostMetaStates";

export interface BundleFilterStates{
    selectedApplicationId: string
    //selectedCostBlockIds: string[]
    //visibleCostBlockIds: string[]
    //costBlocks: BundleFilterCostBlockState[]
    //startDate: string
    //endDate: string
}

export interface BundleFilterCostBlockState{
    costBlockId: string
    selectedCostElementIds: string[]
    visibleCostElementIds: string[]
}

export enum DataLoadingState {
    None,
    WithoutLoading,
    Wait,
    Loaded
}
