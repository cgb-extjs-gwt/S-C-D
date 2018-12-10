import { Reducer, Action } from "redux";
import { 
    COST_APPROVAL_SELECT_APPLICATION,
    COST_APPROVAL_CHECK_COST_BLOCK,
    COST_APPROVAL_CHECK_COST_ELEMENT,
    COST_APPROVAL_UNCHECK_COST_BLOCK,
    COST_APPROVAL_UNCHECK_COST_ELEMENT,
    COST_APPROVAL_SELECT_START_DATE,
    COST_APPROVAL_SELECT_END_DATE, 
    COST_APPROVAL_ON_INIT,
    COST_APPROVAL_CHECK_MULTI_COST_BLOCKS,
    COST_APPROVAL_CHECK_MULTI_COST_ELEMENTS,
} from '../Actions/CostApprovalFilterActions';
import { BundleFilterStates, CostElementId } from '../States/BundleFilterStates'
import { PageInitAction, APP_PAGE_INIT, APP_LOAD_DATA, LoadingAppDataAction } from "../../Layout/Actions/AppActions";
import { ItemSelectedAction, CommonAction, MultiItemSelectedAction } from "../../Common/Actions/CommonActions";
import filter from "../Components/FilterBundlesView";

const initialBundleFilterState = () => {
    const startDateNow = new Date();
    startDateNow.setHours(0,0,0,0);

    const endDateNow = new Date();
    endDateNow.setHours(23,59,59,999);

    const filter: BundleFilterStates = {
        selectedApplicationId: null,
        selectedCostBlockIds: [],
        selectedCostElementIds: [],
        startDate: Ext.Date.add(startDateNow, Ext.Date.MONTH, -2),
        endDate: endDateNow
    }

    return filter;
};

const selectApplication: Reducer<BundleFilterStates, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedApplicationId: action.selectedItemId,
        selectedCostBlockIds: [],
        selectedCostElementIds: []
    }
}

const checkCostBlock: Reducer<BundleFilterStates, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedCostBlockIds: state.selectedCostBlockIds.concat(action.selectedItemId)
    }
}

const checkCostElement: Reducer<BundleFilterStates, CommonAction<CostElementId>> = (state, { data }) => {
    return {
        ...state,
        selectedCostElementIds: state.selectedCostElementIds.concat(data)
    }
}

const unCheckCostBlock: Reducer<BundleFilterStates, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedCostBlockIds: state.selectedCostBlockIds.filter(elem => elem !== action.selectedItemId),
        selectedCostElementIds: state.selectedCostElementIds.filter(elem => elem.costBlockId != action.selectedItemId)
    }
}

const unCheckCostElement: Reducer<BundleFilterStates, CommonAction<CostElementId>> = (state, { data: { costBlockId, costElementId } }) => {
    return {
        ...state,
        selectedCostElementIds: state.selectedCostElementIds.filter(elem => elem.costBlockId != costBlockId || elem.costElementId != costElementId)
    }
}

const selectStartDate: Reducer<BundleFilterStates, CommonAction<Date>> = (state, action) => {
    return {
        ...state,
        startDate: action.data
    }
}

const selectEndDate: Reducer<BundleFilterStates, CommonAction<Date>> = (state, action) => {
    return {
        ...state,
        endDate: new Date(action.data.setHours(23, 59, 59, 999))
    }
}

const init: Reducer<BundleFilterStates, LoadingAppDataAction> = (state, { data }) => {
    const applicationId = data.meta.applications[0].id;
    const costBlock = data.meta.costBlocks.find(item => item.applicationIds.includes(applicationId));

    const filter: BundleFilterStates = {
        ...state,
        selectedApplicationId: applicationId,
        selectedCostBlockIds: [costBlock.id],
        selectedCostElementIds: [{
            costBlockId: costBlock.id,
            costElementId: costBlock.costElements[0].id
        }]
    }

    return filter;
}

const checkMultiCostBlocks: Reducer<BundleFilterStates, MultiItemSelectedAction> = (state, { selectedItemIds }) => ({
    ...state,
    selectedCostBlockIds: selectedItemIds,
    selectedCostElementIds: state.selectedCostElementIds.filter(
        ({ costBlockId }) => selectedItemIds.includes(costBlockId)
    )
})

const checkMultiCostElements: Reducer<BundleFilterStates, CommonAction<CostElementId[]>> = (state, action) => ({
    ...state,
    selectedCostElementIds: action.data
})

export const bundleFilterReducer: Reducer<BundleFilterStates, Action<string>> = (state = initialBundleFilterState(), action) => {
    switch(action.type){
        case COST_APPROVAL_SELECT_APPLICATION:
            return selectApplication(state, <ItemSelectedAction>action);
        case COST_APPROVAL_CHECK_COST_BLOCK:
            return checkCostBlock(state, <ItemSelectedAction>action);
        case COST_APPROVAL_CHECK_COST_ELEMENT:
            return checkCostElement(state, <CommonAction<CostElementId>>action);
        case COST_APPROVAL_UNCHECK_COST_BLOCK:
            return unCheckCostBlock(state, <ItemSelectedAction>action);
        case COST_APPROVAL_UNCHECK_COST_ELEMENT:
            return unCheckCostElement(state, <CommonAction<CostElementId>>action);
        case COST_APPROVAL_SELECT_START_DATE:
            return selectStartDate(state, <CommonAction<Date>>action);
        case COST_APPROVAL_SELECT_END_DATE:
            return selectEndDate(state, <CommonAction<Date>>action);
        case APP_LOAD_DATA:
            return init(state, <LoadingAppDataAction>action);
        case COST_APPROVAL_CHECK_MULTI_COST_BLOCKS:
            return checkMultiCostBlocks(state, <MultiItemSelectedAction>action);
        case COST_APPROVAL_CHECK_MULTI_COST_ELEMENTS:
            return checkMultiCostElements(state, <CommonAction<CostElementId[]>>action);
        default:
            return state;
    }
}
