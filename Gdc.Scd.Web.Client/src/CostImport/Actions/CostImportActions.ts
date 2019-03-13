import { ItemSelectedAction, CommonAction } from "../../Common/Actions/CommonActions";
import { NamedId } from "../../Common/States/CommonStates";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { Action } from "redux";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";

export const COST_IMPORT_SELECT_APPLICATION = 'COST_IMPORT.SELECT.APPLICATION'
export const COST_IMPORT_SELECT_COST_BLOCK = 'COST_IMPORT.SELECT.COST_BLOCK'
export const COST_IMPORT_SELECT_COST_ELEMENT = 'COST_IMPORT.SELECT.COST_ELEMENT'
export const COST_IMPORT_SELECT_INPUT_LEVEL = 'COST_IMPORT.SELECT.INPUT_LEVEL'
export const COST_IMPORT_LOAD_DEPENDENCY_ITEMS = 'COST_IMPORT.LOAD.DEPENDENCY_ITEMS'
export const COST_IMPORT_SELECT_DEPENDENCY_ITEM = 'COST_IMPORT.SELECT.DEPENDENCY_ITEM'
export const COST_IMPORT_LOAD_REGIONS = 'COST_IMPORT.LOAD.REGIONS'
export const COST_IMPORT_SELECT_REGION = 'COST_IMPORT.SELECT.REGION'
export const COST_IMPORT_LOAD_IMPORT_STATUS = 'COST_IMPORT.LOAD.IMPORT_STATUS'
export const COST_IMPORT_SELECT_FILE = 'COST_IMPORT.SELECT.FILE'
export const COST_IMPORT_LOAD_FILE_DATA = 'COST_IMPORT.LOAD.FILE_DATA'
export const COST_IMPORT_LOAD_QUALITY_GATE_ERRORS = 'COST_IMPORT.LOAD.QUALITY_GATE_ERRORS'

export const selectApplication = (applicationId: string) => (<ItemSelectedAction>{
    type: COST_IMPORT_SELECT_APPLICATION,
    selectedItemId: applicationId
})

export const selectCostBlock = (costBlockId: string) => (<ItemSelectedAction>{
    type: COST_IMPORT_SELECT_COST_BLOCK,
    selectedItemId: costBlockId
})

export const selectCostElement = (costElementId: string) => (<ItemSelectedAction>{
    type: COST_IMPORT_SELECT_COST_ELEMENT,
    selectedItemId: costElementId
})

export const selectInputLevel = (inputLevelId: string) => (<ItemSelectedAction>{
    type: COST_IMPORT_SELECT_INPUT_LEVEL,
    selectedItemId: inputLevelId
})

export const selectDependencyItem = (dependencyId: number) => (<ItemSelectedAction<number>>{
    type: COST_IMPORT_SELECT_DEPENDENCY_ITEM,
    selectedItemId: dependencyId
})

export const selectRegion = (regionId: number) => (<ItemSelectedAction<number>>{
    type: COST_IMPORT_SELECT_REGION,
    selectedItemId: regionId
})

export const loadRegions = (regions: NamedId<number>[]) =>(<CommonAction<NamedId<number>[]>>{
    type: COST_IMPORT_LOAD_REGIONS,
    data: regions
})

export const loadDependencyItems = (dependencyItems: NamedId<number>[]) => (<CommonAction<NamedId<number>[]>>{
    type: COST_IMPORT_LOAD_DEPENDENCY_ITEMS,
    data: dependencyItems
})

export const loadImportStatus = (status: string[]) => (<CommonAction<string[]>>{
    type: COST_IMPORT_LOAD_IMPORT_STATUS,
    data: status
})

export const selectFile = (fileName: string) => (<ItemSelectedAction>{
    type: COST_IMPORT_SELECT_FILE,
    selectedItemId: fileName
})

export const loadFileData = (base64Data: string) => (<CommonAction<string>>{
    type: COST_IMPORT_LOAD_FILE_DATA,
    data: base64Data
})

export const loadQualityGateErrors = (qualityGateErrors: BundleDetailGroup[]) => (<CommonAction<BundleDetailGroup[]>>{
    type: COST_IMPORT_LOAD_QUALITY_GATE_ERRORS,
    data: qualityGateErrors
})