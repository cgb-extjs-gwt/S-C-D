import { NamedId } from "./CommonStates";

export interface UsingInfo {
    isUsingCostEditor: boolean
    isUsingTableView: boolean
}

export interface CostElementMeta extends NamedId, UsingInfo {
    dependency: NamedId
    description: string
    inputLevels: InputLevelMeta[]
    regionInput: NamedId
    //inputType: InputType
    typeOptions: {
        Type: FieldType
    }
}

export interface CostBlockMeta extends NamedId {
    applicationIds: string[]
    costElements: CostElementMeta[]
    isUsingCostEditor: boolean
    isUsingTableView: boolean
}

export interface InputLevelMeta extends NamedId {
    levelNumer: number
    isFilterLoading: boolean
}

// export enum InputType {
//     Manually = 0,
//     Automatically = 1,
//     Reference = 2,
//     ManuallyAutomaticly = 3
// }

export enum FieldType {
    Reference = "Reference",
    Double = "Double",
    Flag = "Flag"
}

export interface ApplicationMeta extends NamedId, UsingInfo {

}

export interface CostMetaData {
    applications: ApplicationMeta[]
    costBlocks: CostBlockMeta[]
}

