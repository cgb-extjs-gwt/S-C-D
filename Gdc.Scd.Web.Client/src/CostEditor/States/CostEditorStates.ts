import { CostBlockState } from "./CostBlockStates";
import { Action } from "redux";
import { NamedId } from "../../Common/States/CommonStates";

export interface CostElementMeta extends NamedId {
    dependency: NamedId
    description: string
    inputLevels: InputLevelMeta[]
    regionInput: NamedId
    inputType: InputType
}

export interface CostBlockMeta extends NamedId {
    applicationIds: string[]
    costElements: CostElementMeta[]
}

export interface CostEditortDto {
    applications: NamedId[]
    costBlocks: CostBlockMeta[]
}

export interface InputLevelMeta extends NamedId {
    levelNumer: number
    isFilterLoading: boolean
}

export enum InputType {
    Manually,
    Automatically,
    ManualyAutomaticly,
    Reference
}

export interface CostEditorState {
    applications: Map<string, NamedId>
    costBlockMetas: Map<string, CostBlockMeta>
    selectedApplicationId: string
    costBlocks: CostBlockState[]
    visibleCostBlockIds: string[]
    selectedCostBlockId: string
    dataLossInfo: {
        isWarningDisplayed: boolean
        action: Action<string>
    }
}