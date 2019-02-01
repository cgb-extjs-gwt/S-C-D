import { Reducer, Action } from "redux";
import { COST_APPROVAL_QUALITY_GATE_RESULT_LOAD, COST_APPROVAL_QUALITY_GATE_ERROR_RESET } from "../Actions/QualityGateActions";
import { CommonAction } from "../../Common/Actions/CommonActions";

export const qualityGateErrorsReducer: Reducer<{[key: string]: any}[], Action<string>> = (state = [], action) => {
    switch(action.type){
        case COST_APPROVAL_QUALITY_GATE_RESULT_LOAD:
            return (<CommonAction>action).data;

        case COST_APPROVAL_QUALITY_GATE_ERROR_RESET:
            return [];

        default:
            return state;
    }
}