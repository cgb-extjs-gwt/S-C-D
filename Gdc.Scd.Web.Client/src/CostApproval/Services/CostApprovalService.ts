import { BundleFilterStates } from "../States/BundleFilterStates"
import { NamedId } from "../../Common/States/CommonStates";
import { get, post } from "../../Common/Services/Ajax";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { BuldleFilter } from "../States/BuldleFilter";
import { ApprovalBundle } from "../States/ApprovalBundle";

export const CONTROLLER_NAME = 'CostBlockHistory';

export const getCostApprovalFilterData = () => get<CostMetaData>('CostEditor', 'GetCostEditorData');

export const approve = (historyId: number) => post(CONTROLLER_NAME, 'Approve', null, { historyId });

export const reject = (historyId: number, message: string) => post(CONTROLLER_NAME, 'Reject', null, { historyId, message });

export const getBundles = (filter: BuldleFilter) => get<ApprovalBundle[]>(CONTROLLER_NAME, 'GetDtoHistoriesForApproval', filter);



