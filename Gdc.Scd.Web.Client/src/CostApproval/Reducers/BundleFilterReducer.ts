import { Reducer, Action } from "redux";
import { COST_APPROVAL_PAGE,
         COST_APPROVAL_SELECT_APPLICATION,
         COST_APPROVAL_CHECK_COST_BLOCK,
         COST_APPROVAL_CHECK_COST_ELEMENT,
         COST_APPROVAL_UNCHECK_COST_BLOCK,
         COST_APPROVAL_UNCHECK_COST_ELEMENT,
         COST_APPROVAL_SELECT_START_DATE,
         COST_APPROVAL_SELECT_END_DATE, 
         COST_APPROVAL_APPLY_FILTER} from '../Actions/CostApprovalFilterActions';
import { BundleFilterStates } from '../States/BundleFilterStates'
import { PageInitAction, APP_PAGE_INIT } from "../../Layout/Actions/AppActions";
import { ItemSelectedAction, ItemWithParentSelectedAction, CommonAction } from "../../Common/Actions/CommonActions";
import { ElementWithParent } from '../../Common/States/CommonStates';
import { BudleFilter } from '../../Layout/States/AppStates';
import filter from "../Components/FilterBundlesView";

const now = new Date();

const initialBundleFilterState = () => {
    const filter: BundleFilterStates = {
        selectedApplicationId: "Hardware",
        selectedCostBlockIds: ["FieldServiceCost"],
        selectedCostElementIds: [{
            element: "LabourCost", 
            parentId: "FieldServiceCost"}],
        startDate: new Date(now.setMonth(now.getMonth() - 2)),
        endDate: new Date()
    }

    return <BudleFilter>{
        filter,
        applyFilter: filter
    }
};

const selectApplication: Reducer<BudleFilter, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        filter:{
            ...state.filter,
            selectedApplicationId: action.selectedItemId,
            selectedCostBlockIds: [],
            selectedCostElementIds: []
        }
    }
}

const checkCostBlock: Reducer<BudleFilter, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        filter: {
            ...state.filter,
            selectedCostBlockIds: state.filter.selectedCostBlockIds.concat(action.selectedItemId)
        }
    }
}

const checkCostElement: Reducer<BudleFilter, ItemWithParentSelectedAction> = (state, action) => {
    return {
        ...state,
        filter: {
            ...state.filter,
            selectedCostElementIds: state.filter.selectedCostElementIds.concat(
                <ElementWithParent<string, string>>
                {
                    element: action.selectedItemId, 
                    parentId: action.selectedItemParentId
                }
            )}
        }
}

const unCheckCostBlock: Reducer<BudleFilter, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        filter:{
            ...state.filter,
            selectedCostBlockIds: state.filter.selectedCostBlockIds.filter(elem => elem !== action.selectedItemId),
            selectedCostElementIds: state.filter.selectedCostElementIds.filter(elem => elem.parentId != action.selectedItemId)
        }
        
    }
}

const unCheckCostElement: Reducer<BudleFilter, ItemSelectedAction> = (state, action) => {
    return {
        ...state,
        filter: {
            ...state.filter,
            selectedCostElementIds: state.filter.selectedCostElementIds.filter(elem => elem.parentId != action.selectedItemId)
        }
    }
}

const selectStartDate: Reducer<BudleFilter, CommonAction<Date>> = (state, action) => {
    return {
        ...state,
        filter: {
            ...state.filter,
            startDate: action.data
        }
    }
}

const selectEndDate: Reducer<BudleFilter, CommonAction<Date>> = (state, action) => {
    return {
        ...state,
        filter: {
            ...state.filter,
            endDate: action.data
        }
    }
}

const applyFilter: Reducer<BudleFilter, Action<string>> = (state, action) =>{
    const newState = {
        ...state,
        applyFilter: {...state.filter}
    };
    console.log(newState);
    return newState;
}


export const bundleFilterReducer: Reducer<BudleFilter, Action<string>> = (state = initialBundleFilterState(), action) => {
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
        case COST_APPROVAL_APPLY_FILTER:
            return applyFilter(state, <Action<string>>action);
        default:
            return state;
    }

}
