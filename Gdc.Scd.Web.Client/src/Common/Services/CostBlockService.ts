import { CostElementIdentifier } from "../States/CostElementIdentifier";
import { get, post } from "./Ajax";
import { NamedId } from "../States/CommonStates";
import { ImportResult } from "../../CostImport/States/ImportResult";
import { CostElementData } from "../States/CostElementData";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";

const COST_BLOCK_CONTROLLER = 'CostBlock';

export interface ImportData {
    costElementId: CostElementIdentifier
    approvalOption: ApprovalOption
    dependencyItemId?: number
    regionId?: number
    excelFile: string
}

export const getCostElementData = (costElementId: CostElementIdentifier) => get<CostElementData>(
    COST_BLOCK_CONTROLLER,
    'GetCostElementData',
    costElementId
)

export const importExcel = (importData: ImportData) => post<any, ImportResult>(COST_BLOCK_CONTROLLER, 'ImportExcel', importData)