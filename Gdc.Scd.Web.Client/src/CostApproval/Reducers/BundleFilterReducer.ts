import { Reducer, Action } from "redux";
import { COST_APPROVAL_PAGE,
         COST_APPROVAL_SELECT_APPLICATION,
         COST_APPROVAL_CHECK_COST_BLOCK,
         COST_APPROVAL_CHECK_COST_ELEMENT,
         COST_APPROVAL_UNCHECK_COST_BLOCK,
         COST_APPROVAL_UNCHECK_COST_ELEMENT,
         COST_APPROVAL_SELECT_START_DATE,
         COST_APPROVAL_SELECT_END_DATE, 
         COST_APPROVAL_ON_INIT} from '../Actions/CostApprovalFilterActions';
import { BundleFilterStates } from '../States/BundleFilterStates'
import { PageInitAction, APP_PAGE_INIT, APP_LOAD_META, LoadingMetaDataAction } from "../../Layout/Actions/AppActions";
import { ItemSelectedAction, ItemWithParentSelectedAction, CommonAction } from "../../Common/Actions/CommonActions";
import { ElementWithParent } from '../../Common/States/CommonStates';
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

const checkCostElement: Reducer<BundleFilterStates, ItemWithParentSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedCostElementIds: state.selectedCostElementIds.concat(
            <ElementWithParent<string, string>>
            {
                element: action.selectedItemId, 
                parentId: action.selectedItemParentId
            }
        )
    }
}

const unCheckCostBlock: Reducer<BundleFilterStates, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedCostBlockIds: state.selectedCostBlockIds.filter(elem => elem !== action.selectedItemId),
        selectedCostElementIds: state.selectedCostElementIds.filter(elem => elem.parentId != action.selectedItemId)
    }
}

const unCheckCostElement: Reducer<BundleFilterStates, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        selectedCostElementIds: state.selectedCostElementIds.filter(elem => elem.element != action.selectedItemId)
    }
}

const selectStartDate: Reducer<BundleFilterStates, CommonAction<Date>> = (state, action) => {
    console.log(action.data);
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

const init: Reducer<BundleFilterStates, LoadingMetaDataAction> = (state, { data }) => {
    const applicationId = data.applications[0].id;
    const costBlock = data.costBlocks.find(item => item.applicationIds.includes(applicationId));

    const filter: BundleFilterStates = {
        ...state,
        selectedApplicationId: applicationId,
        selectedCostBlockIds: [costBlock.id],
        selectedCostElementIds: [{
            element: costBlock.costElements[0].id,
            parentId: costBlock.id
        }]
    }

    return filter;
}

export const bundleFilterReducer: Reducer<BundleFilterStates, Action<string>> = (state = initialBundleFilterState(), action) => {
    switch(action.type){
        case COST_APPROVAL_SELECT_APPLICATION:
            return selectApplication(state, <ItemSelectedAction>action);
        case COST_APPROVAL_CHECK_COST_BLOCK:
            return checkCostBlock(state, <ItemSelectedAction>action);
        case COST_APPROVAL_CHECK_COST_ELEMENT:
            return checkCostElement(state, <ItemWithParentSelectedAction>action);
        case COST_APPROVAL_UNCHECK_COST_BLOCK:
            return unCheckCostBlock(state, <ItemSelectedAction>action);
        case COST_APPROVAL_UNCHECK_COST_ELEMENT:
            return unCheckCostElement(state, <ItemSelectedAction>action);
        case COST_APPROVAL_SELECT_START_DATE:
            return selectStartDate(state, <CommonAction<Date>>action);
        case COST_APPROVAL_SELECT_END_DATE:
            return selectEndDate(state, <CommonAction<Date>>action);
        case APP_LOAD_META:
            return init(state, <LoadingMetaDataAction>action);
        default:
            return state;
    }
}
