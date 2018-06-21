import { CostBlockState } from "./CostBlockStates";
import { Action } from "redux";
import { NamedId } from "../../Common/States/CommonStates";

export interface CostElementMeta extends NamedId {
    dependency: NamedId
    description: string
    scopeId: string
}

export interface CostBlockMeta extends NamedId {
    applicationIds: string[]
    costElements: CostElementMeta[]
}

export interface CostEdirotDto {
    countries: NamedId[]
    meta: {
        applications: NamedId[]
        scopes: NamedId[]
        costBlocks: CostBlockMeta[]
        inputLevels: NamedId[]
    }
}

export interface InputLevelMeta extends NamedId {
    levelNumer: number
    isFilterLoading: boolean
}

export interface CostEditorState {
    applications: Map<string, NamedId>
    scopes: Map<string, NamedId>
    countries: Map<string, NamedId>
    costBlockMetas: Map<string, CostBlockMeta>
    inputLevels: Map<string, InputLevelMeta> 
    selectedApplicationId: string
    selectedScopeId: string
    costBlocks: CostBlockState[]
    visibleCostBlockIds: string[]
    selectedCostBlockId: string

    dataLossInfo: {
        isWarningDisplayed: boolean
        action: Action<string>
    }
}