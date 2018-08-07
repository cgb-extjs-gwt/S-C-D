import { Action } from 'redux';
import { asyncAction, AsyncAction } from "../../Common/Actions/AsyncAction";
import * as service from "../Services/CostApprovalService";
import { ItemSelectedAction, ItemsSelectedAction } from "../../Common/Actions/CommonActions";
import { openPage, loadMetaData } from '../../Layout/Actions/AppActions';

export const COST_APPROVAL_PAGE = "costApproval";
export const COST_APPROVAL_SELECT_APPLICATION = "COST_APPROVAL_SELECT_APPLICATIONS";
export const COST_APPROVAL_CHECK_COST_BLOCK = "COST_APPROVAL_CHECK_COST_BLOCK";
export const COST_APPROVAL_CHECK_COST_ELEMENT = "COST_APPROVAL_CHECK_COST_ELEMENT";
export const COST_APPROVAL_UNCHECK_COST_BLOCK = "COST_APPROVAL_UNCHECK_COST_BLOCK";
export const COST_APPROVAL_UNCHECK_COST_ELEMENT = "COST_APPROVAL_UNCHECK_COST_ELEMENT";
export const COST_APPROVAL_SELECT_START_DATE = "COST_APPROVAL_SELECT_START_DATE";
export const COST_APPROVAL_SELECT_END_DATE = "COST_APPROVAL_SELECT_END_DATE";



export const init = () => asyncAction(
    dispatch => {
        dispatch(openPage(COST_APPROVAL_PAGE, 'Cost Elements Approval'));
        service.getCostApprovalFilterData().then(
            data => {
                //TODO: remove
                console.log(data);
                dispatch(loadMetaData(data));
            } 
        );
        error => dispatch(error(error))
    }
)



