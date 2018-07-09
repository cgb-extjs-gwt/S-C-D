import { CostBlockState } from "./CostBlockStates";
import { Action } from "redux";
import { NamedId } from "../../Common/States/CommonStates";

export interface CostElementMeta extends NamedId {
    dependency: NamedId
    description: string
    inputLevels: NamedId[]
    regionInput: NamedId
}

export interface CostBlockMeta extends NamedId {
    applicationIds: string[]
    costElements: CostElementMeta[]
}

export interface CostEdirotDto {
    applications: NamedId[]
    costBlocks: CostBlockMeta[]
}

export interface InputLevelMeta extends NamedId {
    levelNumer: number
    isFilterLoading: boolean
}

export interface CostEditorState {
    applications: Map<string, NamedId>
    countries: Map<string, NamedId>
    costBlockMetas: Map<string, CostBlockMeta>
    inputLevels: Map<string, InputLevelMeta> 
    selectedApplicationId: string
    costBlocks: CostBlockState[]
    visibleCostBlockIds: string[]
    selectedCostBlockId: string
    dataLossInfo: {
        isWarningDisplayed: boolean
        action: Action<string>
    }
}