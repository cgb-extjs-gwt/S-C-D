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
    applications: NamedId[]
    scopes: NamedId[]
    countries: NamedId[]
    costBlockMetas: CostBlockMeta[]
    inputLevels: NamedId[]
}

export interface CostEditorState {
    applications: Map<string, NamedId>
    scopes: Map<string, NamedId>
    countries: Map<string, NamedId>
    costBlockMetas: Map<string, CostBlockMeta>
    inputLevels: Map<string, NamedId> 
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