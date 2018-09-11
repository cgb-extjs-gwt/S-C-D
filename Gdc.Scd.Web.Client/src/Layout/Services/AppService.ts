import { CostMetaData } from "../../Common/States/CostMetaStates";
import { get } from "../../Common/Services/Ajax";

export const getCostMetaData = () => get<CostMetaData>('CostEditor', 'GetCostEditorData');