import { CostElementIdentifier } from "../States/CostElementIdentifier";
import { get, post } from "./Ajax";
import { NamedId } from "../States/CommonStates";
import { ImportResult } from "../../CostImport/States/ImportResult";
import { ImportData } from "../States/ImportData";

const COST_BLOCK_CONTROLLER = 'CostBlock';

export const getRegions = (costElementId: CostElementIdentifier, regionInputId?: number) => get<NamedId<number>[]>(
    COST_BLOCK_CONTROLLER, 
    'GetRegions',
    { 
        ...costElementId,
        regionInputId
    }
)

export const getDependencyItems = (costElementId: CostElementIdentifier, regionInputId?: number) => get<NamedId<number>[]>(
    COST_BLOCK_CONTROLLER, 
    'GetDependencyItems',
    { 
        ...costElementId,
        regionInputId
    }
)

export const importExcel = (importData: ImportData) => post<any, ImportResult>(COST_BLOCK_CONTROLLER, 'ImportExcel', importData)