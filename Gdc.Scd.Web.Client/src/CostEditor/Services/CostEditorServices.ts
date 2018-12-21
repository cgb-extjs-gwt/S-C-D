import { buildMvcUrl, get, post } from "../../Common/Services/Ajax";
import { NamedId } from "../../Common/States/CommonStates";
import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";
import { CostElementData, EditItem } from "../States/CostBlockStates";
import { Context } from "../../Common/States/Context";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";

export const COST_EDITOR_CONTROLLER_NAME = 'CostEditor';

export const getCostElementData = (context: Context) => get<CostElementData>(COST_EDITOR_CONTROLLER_NAME, 'GetCostElementData', context);

export const getLevelInputFilterItems = (context: Context) => 
    get<NamedId[]>(COST_EDITOR_CONTROLLER_NAME, 'GetInputLevelFilterItems', context);  

export const buildGetEditItemsUrl = (context: Context) => 
    buildMvcUrl(COST_EDITOR_CONTROLLER_NAME, 'GetEditItems', context);

export const saveEditItems = (editItems: EditItem[], context: Context, approvalOption: ApprovalOption) =>
    post<any, QualityGateResult>(COST_EDITOR_CONTROLLER_NAME, 'UpdateValues', editItems, { ...context, ...approvalOption });

export const buildGetHistoryUrl = (context: Context, editItemId: string) => 
    buildMvcUrl(COST_EDITOR_CONTROLLER_NAME, 'GetHistory', { ...context, editItemId });