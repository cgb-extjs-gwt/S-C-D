import { Action } from "redux";
import { CommonAction } from "../../Common/Actions/CommonActions";

export const COST_APPROVAL_QUALITY_GATE_RESULT_LOAD = 'COST_APPROVAL.QUALITY_GATE_RESULT.LOAD'
export const COST_APPROVAL_QUALITY_GATE_ERROR_RESET = 'COST_APPROVAL.QUALITY_GATE.ERROR_RESET'

export const resetQualityGateErrors = () => <Action<string>>{ type: COST_APPROVAL_QUALITY_GATE_ERROR_RESET };

export const loadQualityGateResult = (qualityGateResult: {[key: string]: any}[]) => <CommonAction<{[key: string]: any}[]>>{ 
    type: COST_APPROVAL_QUALITY_GATE_RESULT_LOAD, 
    data: qualityGateResult
}