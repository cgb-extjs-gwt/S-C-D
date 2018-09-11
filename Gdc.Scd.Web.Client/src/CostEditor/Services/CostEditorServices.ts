import { CostEditorState, CostEditortData } from "../States/CostEditorStates";
import { CostBlockState, EditItem, CostElementData } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { get, post, buildMvcUrl } from "../../Common/Services/Ajax";
import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";

export const COST_EDITOR_CONTROLLER_NAME = 'CostEditor';

const COST_BLOCK_HISTORY_CONTROLLER_NAME = 'CostBlockHistory';

export interface Context {
    applicationId: string
    scopeId: string
    regionInputId: string
    costBlockId: string
    costElementId: string
    inputLevelId: string
    costElementFilterIds: string[],
    inputLevelFilterIds: string[]
}

export interface ApprovalOption {
    isApproving?: boolean
    hasQualityGateErrors?: boolean
    qualityGateErrorExplanation?: string
}

export const getCostEditorData = () => get<CostEditortData>(COST_EDITOR_CONTROLLER_NAME, 'GetCostEditorData');

export const getCostElementData = (context: Context) => get<CostElementData>(COST_EDITOR_CONTROLLER_NAME, 'GetCostElementData', context);

export const getLevelInputFilterItems = (context: Context) => 
    get<NamedId[]>(COST_EDITOR_CONTROLLER_NAME, 'GetInputLevelFilterItems', context);  

export const getEditItems = (context: Context) => 
    get<EditItem[]>(COST_EDITOR_CONTROLLER_NAME, 'GetEditItems', context); 

export const saveEditItems = (editItems: EditItem[], context: Context, approvalOption: ApprovalOption) =>
    post<any, QualityGateResult>(COST_EDITOR_CONTROLLER_NAME, 'UpdateValues', editItems, { ...context, ...approvalOption });

export const buildGetHistoryUrl = (context: Context, editItemId: string) => 
    buildMvcUrl(COST_BLOCK_HISTORY_CONTROLLER_NAME, 'GetHistory', { ...context, editItemId });