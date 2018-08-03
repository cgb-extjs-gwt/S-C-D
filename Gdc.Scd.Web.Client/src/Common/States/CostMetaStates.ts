import { NamedId } from "./CommonStates";

export interface InputLevelMeta extends NamedId{
    levelNumer: number
    isFilterLoading: boolean
}

export enum InputType {
    Manually = 0,
    Automatically = 1,
    Reference = 2,
    ManuallyAutomaticly = 3
}

export enum FieldType {
    Reference = "Reference",
    Double = "Double"
}

export interface InputLevelMeta extends NamedId {
    levelNumer: number
    isFilterLoading: boolean
}

export interface CostElementMeta extends NamedId{
    dependency: NamedId
    description: string
    inputLevels: InputLevelMeta[]
    regionInput: NamedId
    inputType: InputType
    typeOptions: {
        Type: FieldType
    }
}

export interface CostBlockMeta extends NamedId{
    applicationIds: string[]
    costElements: CostElementMeta[]
}

export interface CostMetaData{
    applications: NamedId[]
    costBlocks: CostBlockMeta[]
}

