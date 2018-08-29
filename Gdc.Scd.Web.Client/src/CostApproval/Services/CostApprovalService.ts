import { BundleFilterStates } from "../States/BundleFilterStates"
import { NamedId } from "../../Common/States/CommonStates";
import { get, post, buildMvcUrl } from "../../Common/Services/Ajax";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { ApprovalBundle } from "../States/ApprovalBundle";
import { BundleFilter } from "../States/BundleFilter";

export const CONTROLLER_NAME = 'CostBlockHistory';

export const getCostApprovalFilterData = () => get<CostMetaData>('CostEditor', 'GetCostEditorData');

export const approve = (historyId: number) => post(CONTROLLER_NAME, 'Approve', null, { historyId });

export const reject = (historyId: number, message: string) => post(CONTROLLER_NAME, 'Reject', null, { historyId, message });

export const getBundles = (filter: BundleFilter) => get<ApprovalBundle[]>(CONTROLLER_NAME, 'GetApprovalBundles', filter);

export const buildGetApproveBundleDetailUrl = (bundleId: number) => 
    buildMvcUrl(CONTROLLER_NAME, 'GetApproveBundleDetail', { costBlockHistoryId: bundleId });



