import { Reducer, Action } from "redux";
import { 
    APPROVAL_FILTER_SELECT_APPLICATION,
    APPROVAL_FILTER_CHECK_COST_BLOCK,
    APPROVAL_FILTER_CHECK_COST_ELEMENT,
    APPROVAL_FILTER_UNCHECK_COST_BLOCK,
    APPROVAL_FILTER_UNCHECK_COST_ELEMENT,
    APPROVAL_FILTER_SELECT_START_DATE,
    APPROVAL_FILTER_SELECT_END_DATE, 
    APPROVAL_FILTER_ON_INIT,
    APPROVAL_FILTER_CHECK_MULTI_COST_BLOCKS,
    APPROVAL_FILTER_CHECK_MULTI_COST_ELEMENTS,
    APPROVAL_FILTER_SELECT_STATE,
} from '../Actions/FilterActions';
import { PageInitAction, APP_PAGE_INIT, APP_LOAD_DATA, LoadingAppDataAction } from "../../Layout/Actions/AppActions";
import { ItemSelectedAction, CommonAction, MultiItemSelectedAction } from "../../Common/Actions/CommonActions";
import { FilterState, CostElementId, ApprovalBundleState } from "../States/ApprovalState";

const initState = (selectedState: ApprovalBundleState) => {
    const startDateNow = new Date();
    startDateNow.setHours(0,0,0,0);

    const endDateNow = new Date();
    endDateNow.setHours(23,59,59,999);

    const filter: FilterState = {
        selectedApplicationId: null,
        selectedCostBlockIds: [],
        selectedCostElementIds: [],
        startDate: Ext.Date.add(startDateNow, Ext.Date.MONTH, -2),
        endDate: endDateNow,
        selectedState
    }

    return filter;
};

const selectApplication: Reducer<FilterState, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedApplicationId: action.selectedItemId,
        selectedCostBlockIds: [],
        selectedCostElementIds: []
    }
}

const checkCostBlock: Reducer<FilterState, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedCostBlockIds: state.selectedCostBlockIds.concat(action.selectedItemId)
    }
}

const checkCostElement: Reducer<FilterState, CommonAction<CostElementId>> = (state, { data }) => {
    return {
        ...state,
        selectedCostElementIds: state.selectedCostElementIds.concat(data)
    }
}

const unCheckCostBlock: Reducer<FilterState, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedCostBlockIds: state.selectedCostBlockIds.filter(elem => elem !== action.selectedItemId),
        selectedCostElementIds: state.selectedCostElementIds.filter(elem => elem.costBlockId != action.selectedItemId)
    }
}

const unCheckCostElement: Reducer<FilterState, CommonAction<CostElementId>> = (state, { data: { costBlockId, costElementId } }) => {
    return {
        ...state,
        selectedCostElementIds: state.selectedCostElementIds.filter(elem => elem.costBlockId != costBlockId || elem.costElementId != costElementId)
    }
}

const selectStartDate: Reducer<FilterState, CommonAction<Date>> = (state, action) => {
    return {
        ...state,
        startDate: action.data
    }
}

const selectEndDate: Reducer<FilterState, CommonAction<Date>> = (state, action) => {
    return {
        ...state,
        endDate: new Date(action.data.setHours(23, 59, 59, 999))
    }
}

const init: Reducer<FilterState, LoadingAppDataAction> = (state, { data }) => {
    if (data.meta.applications.length < 1) return null;
    const applicationId = data.meta.applications[0].id;
    const costBlock = data.meta.costBlocks.find(item => item.applicationIds.includes(applicationId));

    const filter: FilterState = {
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

const checkMultiCostBlocks: Reducer<FilterState, MultiItemSelectedAction> = (state, { selectedItemIds }) => ({
    ...state,
    selectedCostBlockIds: selectedItemIds,
    selectedCostElementIds: state.selectedCostElementIds.filter(
        ({ costBlockId }) => selectedItemIds.includes(costBlockId)
    )
})

const checkMultiCostElements: Reducer<FilterState, CommonAction<CostElementId[]>> = (state, action) => ({
    ...state,
    selectedCostElementIds: action.data
})

const selectState: Reducer<FilterState, ItemSelectedAction<ApprovalBundleState>> = (state, { selectedItemId }) => ({
    ...state,
    selectedState: selectedItemId
})

export const buildFilterReducer = (initSelectedState: ApprovalBundleState) => {
    return (state = initState(initSelectedState), action) => {
        switch(action.type){
            case APPROVAL_FILTER_SELECT_APPLICATION:
                return selectApplication(state, <ItemSelectedAction>action);
            case APPROVAL_FILTER_CHECK_COST_BLOCK:
                return checkCostBlock(state, <ItemSelectedAction>action);
            case APPROVAL_FILTER_CHECK_COST_ELEMENT:
                return checkCostElement(state, <CommonAction<CostElementId>>action);
            case APPROVAL_FILTER_UNCHECK_COST_BLOCK:
                return unCheckCostBlock(state, <ItemSelectedAction>action);
            case APPROVAL_FILTER_UNCHECK_COST_ELEMENT:
                return unCheckCostElement(state, <CommonAction<CostElementId>>action);
            case APPROVAL_FILTER_SELECT_START_DATE:
                return selectStartDate(state, <CommonAction<Date>>action);
            case APPROVAL_FILTER_SELECT_END_DATE:
                return selectEndDate(state, <CommonAction<Date>>action);
            case APP_LOAD_DATA:
                return init(state, <LoadingAppDataAction>action);
            case APPROVAL_FILTER_CHECK_MULTI_COST_BLOCKS:
                return checkMultiCostBlocks(state, <MultiItemSelectedAction>action);
            case APPROVAL_FILTER_CHECK_MULTI_COST_ELEMENTS:
                return checkMultiCostElements(state, <CommonAction<CostElementId[]>>action);
            case APPROVAL_FILTER_SELECT_STATE:
                return selectState(state, <ItemSelectedAction<ApprovalBundleState>>action);
            default:
                return state;
        }
    }
}
