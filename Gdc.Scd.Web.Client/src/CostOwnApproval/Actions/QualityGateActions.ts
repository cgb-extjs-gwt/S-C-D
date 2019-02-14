import { Action } from "redux";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";

export const COST_APPROVAL_QUALITY_GATE_RESULT_LOAD = 'COST_APPROVAL.QUALITY_GATE_RESULT.LOAD'
export const COST_APPROVAL_QUALITY_GATE_ERROR_RESET = 'COST_APPROVAL.QUALITY_GATE.ERROR_RESET'

export const resetQualityGateErrors = () => <Action<string>>{ type: COST_APPROVAL_QUALITY_GATE_ERROR_RESET };

export const loadQualityGateResult = (qualityGateResult: BundleDetailGroup[]) => <CommonAction<BundleDetailGroup[]>>{ 
    type: COST_APPROVAL_QUALITY_GATE_RESULT_LOAD, 
    data: qualityGateResult
}