import { BundleFilterStates } from "../States/BundleFilterStates"
import { NamedId } from "../../Common/States/CommonStates";
import { get, post } from "../../Common/Services/Ajax";
import { CostMetaData } from "../../Common/States/CostMetaStates";

const CONTROLLER_NAME = 'CostEditor';

export const getCostApprovalFilterData = () => get<CostMetaData>(CONTROLLER_NAME, 'GetCostEditorData');



