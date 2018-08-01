import { Action } from 'redux';
import { asyncAction, AsyncAction } from "../../Common/Actions/AsyncAction";
import * as service from "../../CostEditor/Services/CostEditorServices";
import { CostEditorState } from "../../CostEditor/States/CostEditorStates";
import { EditItem, CostElementData, DataLoadingState } from "../../CostEditor/States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { CommonState } from "../../Layout/States/AppStates";
import { ItemSelectedAction, ItemsSelectedAction } from "../../Common/Actions/CommonActions";
import { openPage, pageInit } from '../../Layout/Actions/AppActions';

export const COST_APPROVAL_PAGE = "costApproval";
export const COST_APPROVAL_SELECT_APPLICATION = "COST_APPROVAL_SELECT_APPLICATIONS";
export const COST_APPROVAL_SELECT_COST_BLOCKS = "COST_APPROVAL_SELECT_COST_BLOCKS";
export const COST_APPROVAL_SELECT_COST_ELEMENTS = "COST_APPROVAL_SELECT_COST_ELEMENTS";
export const COST_APPROVAL_SELECT_PERIOD = "COST_APPROVAL_SELECT_PERIOD";

export interface SelectPeriodAction extends Action<string>{
    startDate: string;
    endDate: string;
}

export const selectApplication = (applicationId: string) => <ItemSelectedAction>{
    type: COST_APPROVAL_SELECT_APPLICATION,
    selectedItemId: applicationId
}

export const selectCostBlocks = (selectedCostBlockIds: string[]) => <ItemsSelectedAction>{
    type: COST_APPROVAL_SELECT_COST_BLOCKS,
    selectedItemIds: selectedCostBlockIds
}

export const selectCostElements = (selectedCostElementIds: string[]) => <ItemsSelectedAction>{
    type: COST_APPROVAL_SELECT_COST_ELEMENTS,
    selectedItemIds: selectedCostElementIds
}

export const selectTimePeriod = (startDate: string, endDate: string) => <SelectPeriodAction>{
    type: COST_APPROVAL_SELECT_PERIOD,
    startDate: startDate,
    endDate: endDate
}

export const init = () => asyncAction(
    dispatch => {
        dispatch(openPage(COST_APPROVAL_PAGE, 'Cost Elements Approval'));
        service.getCostEditorData().then(
            data => {
                console.log(data);
                dispatch(pageInit(COST_APPROVAL_PAGE, data));
            } 
        );
        error => dispatch(error(error))
    }
)



