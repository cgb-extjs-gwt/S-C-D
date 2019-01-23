import { Reducer, Action } from "redux";
import { CostImportState } from "../States/CostImportState";
import { COST_IMPORT_SELECT_APPLICATION, COST_IMPORT_SELECT_COST_BLOCK, COST_IMPORT_SELECT_COST_ELEMENT, COST_IMPORT_LOAD_COST_ELEMENT_DATA, COST_IMPORT_LOAD_IMPORT_STATUS, COST_IMPORT_SELECT_DEPENDENCY_ITEM, COST_IMPORT_SELECT_FILE, COST_IMPORT_SELECT_REGION } from "../Actions/CostImportActions";
import { ItemSelectedAction, CommonAction } from "../../Common/Actions/CommonActions";
import { CostElementData } from "../../Common/States/CostElementData";
import { SelectList, NamedId } from "../../Common/States/CommonStates";

const reset = () => ({
    dependencyItems: <SelectList<NamedId<number>, number>>{
        list: [],
        selectedItemId: null
    },
    regions: <SelectList<NamedId<number>, number>>{
        list: [],
        selectedItemId: null
    },
    status: []
})

const defaultState = () => (<CostImportState>{
    ...reset(),
    applicationId: null,
    costBlockId: null,
    costElementId: null,
})

const selectApplication: Reducer<CostImportState, ItemSelectedAction> = (state, action) => ({
    ...state,
    ...reset(),
    applicationId: action.selectedItemId,
    costBlockId: null,
    costElementId: null,
})

const selectCostBlock: Reducer<CostImportState, ItemSelectedAction> = (state, action) => ({
    ...state,
    ...reset(),
    costBlockId: action.selectedItemId,
    costElementId: null,
})

const selectCostElement: Reducer<CostImportState, ItemSelectedAction> = (state, action) => ({
    ...state,
    ...reset(),
     costElementId: action.selectedItemId
})

const selectDependencyItem: Reducer<CostImportState, ItemSelectedAction<number>> = (state, action) => ({
    ...state,
    dependencyItems: {
        ...state.dependencyItems,
        selectedItemId: action.selectedItemId
    }
}) 

const selectRegion: Reducer<CostImportState, ItemSelectedAction<number>> = (state, action) => ({
    ...state,
    regions: {
        ...state.dependencyItems,
        selectedItemId: action.selectedItemId
    }
}) 

const loadCostElementData: Reducer<CostImportState, CommonAction<CostElementData>> = (state, { data: { dependencyItems, regions } }) => ({
    ...state,
    dependencyItems: {
        ...state.dependencyItems,
        list: dependencyItems
    },
    regions: {
        ...state.regions,
        list: regions
    }
}) 

const loadImportStatus: Reducer<CostImportState, CommonAction<string[]>> = (state, action) => ({
    ...state,
    status: action.data
}) 

const selectFile: Reducer<CostImportState, ItemSelectedAction> = (state, action) => ({
    ...state,
    fileName: action.selectedItemId
})

export const costImportReducer: Reducer<CostImportState, Action<string>> = (state = defaultState(), action) => {
    switch (action.type) {
        case COST_IMPORT_SELECT_APPLICATION:
            return selectApplication(state, <ItemSelectedAction>action);
        
        case COST_IMPORT_SELECT_COST_BLOCK:
            return selectCostBlock(state, <ItemSelectedAction>action);

        case COST_IMPORT_SELECT_COST_ELEMENT:
            return selectCostElement(state, <ItemSelectedAction>action);

        case COST_IMPORT_SELECT_DEPENDENCY_ITEM:
            return selectDependencyItem(state, <ItemSelectedAction<number>>action);

        case COST_IMPORT_SELECT_REGION:
            return selectRegion(state, <ItemSelectedAction<number>>action);

        case COST_IMPORT_LOAD_COST_ELEMENT_DATA:
            return loadCostElementData(state, <CommonAction<CostElementData>>action);

        case COST_IMPORT_LOAD_IMPORT_STATUS:
            return loadImportStatus(state, <CommonAction<string[]>>action);

        case COST_IMPORT_SELECT_FILE:
            return selectFile(state, <ItemSelectedAction>action);

        default:
            return state;
    }
}