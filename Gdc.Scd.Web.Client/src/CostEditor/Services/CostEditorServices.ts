import { CostEditorState, CostEditortData } from "../States/CostEditorStates";
import { CostBlockState, EditItem, CostElementData } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { get, post } from "../../Common/Services/Ajax";

const CONTROLLER_NAME = 'CostEditor';

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

export const getCostEditorData = () => get<CostEditortData>(CONTROLLER_NAME, 'GetCostEditorData');

export const getCostElementData = (context: Context) => get<CostElementData>(CONTROLLER_NAME, 'GetCostElementData', context);

export const getLevelInputFilterItems = (context: Context) => 
    get<NamedId[]>(CONTROLLER_NAME, 'GetInputLevelFilterItems', context);  

export const getEditItems = (context: Context) => 
    get<EditItem[]>(CONTROLLER_NAME, 'GetEditItems', context); 

export const saveEditItems = (editItems: EditItem[], context: Context, forApproval: boolean) =>
    post(CONTROLLER_NAME, 'UpdateValues', editItems, { ...context, forApproval });