import { ItemSelectedAction, CommonAction } from "../../Common/Actions/CommonActions";
import { NamedId } from "../../Common/States/CommonStates";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { getCostElementData } from "../../Common/Services/CostBlockService";
import { Action } from "redux";
import { CostElementData } from "../../Common/States/CostElementData";

export const COST_IMPORT_SELECT_APPLICATION = 'COST_IMPORT.SELECT.APPLICATION';
export const COST_IMPORT_SELECT_COST_BLOCK = 'COST_IMPORT.SELECT.COST_BLOCK';
export const COST_IMPORT_SELECT_COST_ELEMENT = 'COST_IMPORT.SELECT.COST_ELEMENT';
export const COST_IMPORT_SELECT_DEPENDENCY_ITEM = 'COST_IMPORT.SELECT.DEPENDENCY_ITEM';
export const COST_IMPORT_SELECT_REGION = 'COST_IMPORT.SELECT.REGION';
export const COST_IMPORT_LOAD_COST_ELEMENT_DATA = 'COST_IMPORT.LOAD.COST_ELEMENT_DATA';
export const COST_IMPORT_LOAD_IMPORT_STATUS = 'COST_IMPORT.LOAD.IMPORT_STATUS';
export const COST_IMPORT_SELECT_FILE = 'COST_IMPORT.SELECT.FILE';

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

export const selectDependencyItem = (dependencyId: number) => (<ItemSelectedAction<number>>{
    type: COST_IMPORT_SELECT_DEPENDENCY_ITEM,
    selectedItemId: dependencyId
})

export const selectRegion = (regionId: number) => (<ItemSelectedAction<number>>{
    type: COST_IMPORT_SELECT_REGION,
    selectedItemId: regionId
})

export const loadCostElementData = (costElementData: CostElementData) => (<CommonAction<CostElementData>>{
    type: COST_IMPORT_LOAD_COST_ELEMENT_DATA,
    data: costElementData
})

export const loadImportStatus = (status: string[]) => (<CommonAction<string[]>>{
    type: COST_IMPORT_LOAD_IMPORT_STATUS,
    data: status
})

export const selectFile = (fileName: string) => (<ItemSelectedAction>{
    type: COST_IMPORT_SELECT_FILE,
    selectedItemId: fileName
})