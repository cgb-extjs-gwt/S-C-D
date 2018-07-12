import { CostEditorState, CostEditortDto } from "../States/CostEditorStates";
import { CostBlockState, EditItem } from "../States/CostBlockStates";
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

export const getCostEditorDto = () => get<CostEditortDto>(CONTROLLER_NAME, 'GetCostEditorData');

export const getRegions = (context: Context) => get<NamedId[]>(CONTROLLER_NAME, 'GetRegions', context);

export const getCostElementFilterItems = (context: Context) => 
    get<NamedId[]>(CONTROLLER_NAME, 'GetCostElementFilterItems', context); 

export const getLevelInputFilterItems = (context: Context) => 
    get<NamedId[]>(CONTROLLER_NAME, 'GetInputLevelFilterItems', context);  

export const getEditItems = (context: Context) => 
    get<EditItem[]>(CONTROLLER_NAME, 'GetEditItems', context); 

export const saveEditItems = (editItems: EditItem[], context: Context) =>
    post(CONTROLLER_NAME, 'UpdateValues', editItems, context);