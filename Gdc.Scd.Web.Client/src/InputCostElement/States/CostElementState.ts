import { SelectList } from "../../Common/States/SelectList";
import { CostBlockInputState } from "./CostBlock";
import { NamedId } from "../../Common/States/NamedId";

// export interface InputLevel extends NamedId {
// }

export interface CostElementMeta extends NamedId {
    dependency: NamedId
    description: string
    scopeId: string
}

export interface CostBlockMeta extends NamedId {
    applicationIds: string[]
    costElements: CostElementMeta[]
}

export interface CostElementInputDto {
    applications: NamedId[]
    scopes: NamedId[]
    countries: NamedId[]
    costBlockMetas: CostBlockMeta[]
    inputLevels: NamedId[]
}

export interface CostElementInputState {
    applications: Map<string, NamedId>
    scopes: Map<string, NamedId>
    countries: Map<string, NamedId>
    costBlockMetas: Map<string, CostBlockMeta>
    inputLevels: Map<string, NamedId> 
    selectedApplicationId: string
    selectedScopeId: string
    costBlocksInputs: CostBlockInputState[]
    visibleCostBlockIds: string[]
    selectedCostBlockId: string
    isDataLossWarningDisplayed: boolean
}