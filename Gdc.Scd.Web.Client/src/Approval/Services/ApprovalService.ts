import { NamedId } from "../../Common/States/CommonStates";
import { get, post, buildMvcUrl } from "../../Common/Services/Ajax";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";
import { Bundle, ApprovalBundleState } from "../States/ApprovalState";

export const CONTROLLER_NAME = 'Approval';

export interface ApprovalBundleFilter {
    dateTimeFrom?: Date
    dateTimeTo?: Date
    applicationIds?: string[]
    costBlockIds?: string[]
    costElementIds?: string[]
    userIds?: number[]
    state: ApprovalBundleState
}

export const approve = (historyId: number) => post(CONTROLLER_NAME, 'Approve', null, { historyId });

export const reject = (historyId: number, message?: string) => post(CONTROLLER_NAME, 'Reject', null, { historyId, message });

export const getBundles = (filter: ApprovalBundleFilter) => post<ApprovalBundleFilter, Bundle[]>(CONTROLLER_NAME, 'GetApprovalBundlesByFilter', filter);

export const getOwnBundles = (filter: ApprovalBundleFilter) => post<ApprovalBundleFilter, Bundle[]>(CONTROLLER_NAME, 'GetOwnApprovalBundlesByFilter', filter);

export const buildGetApproveBundleDetailUrl = (
    bundleId: number, 
    historyValueId?: number, 
    costBlockFilter?: { [key: string]: number[] }
) => {
    const params: any = {
        costBlockHistoryId: bundleId, 
        historyValueId
    };

    if (costBlockFilter) {
        params.costBlockFilter = JSON.stringify(costBlockFilter);
    }

    return buildMvcUrl(CONTROLLER_NAME, 'GetApproveBundleDetail', params);
}

export const sendForApproval = (historyId: number, qualityGateErrorExplanation?: string) => 
    get<QualityGateResult>(CONTROLLER_NAME, 'SendForApproval', { historyId, qualityGateErrorExplanation });
