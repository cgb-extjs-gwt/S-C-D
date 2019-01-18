import { ItemSelectedAction, CommonAction } from "../../Common/Actions/CommonActions";
import { NamedId } from "../../Common/States/CommonStates";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { getDependencyItems } from "../../Common/Services/CostBlockService";
import { Action } from "redux";

export const COST_IMPORT_SELECT_APPLICATION = 'COST_IMPORT.SELECT.APPLICATION';
export const COST_IMPORT_SELECT_COST_BLOCK = 'COST_IMPORT.SELECT.COST_BLOCK';
export const COST_IMPORT_SELECT_COST_ELEMENT = 'COST_IMPORT.SELECT.COST_ELEMENT';
export const COST_IMPORT_SELECT_DEPENDENCY_ITEM = 'COST_IMPORT.SELECT.DEPENDENCY_ITEM';
export const COST_IMPORT_LOAD_DEPENDENCY_ITEMS = 'COST_IMPORT.LOAD.DEPENDENCY_ITEMS';
export const COST_IMPORT_LOAD_IMPORT_STATUS = 'COST_IMPORT.LOAD.IMPORT_STATUS';
export const COST_IMPORT_SELECT_FILE = 'COST_IMPORT.SELECT.FILE';

export interface LoadDependencyItemsAction extends Action<string> {
    dependencyItems: NamedId<number>[]
}

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

export const loadDependencyItems = (dependencyItems: NamedId<number>[]) => (<LoadDependencyItemsAction>{
    type: COST_IMPORT_LOAD_DEPENDENCY_ITEMS,
    dependencyItems
})

export const loadImportStatus = (status: string[]) => (<CommonAction<string[]>>{
    type: COST_IMPORT_LOAD_IMPORT_STATUS,
    data: status
})

export const selectFile = (fileName: string) => (<ItemSelectedAction>{
    type: COST_IMPORT_SELECT_FILE,
    selectedItemId: fileName
})