import { CostElementIdentifier } from "./CostElementIdentifier";

export interface Context extends CostElementIdentifier {
    scopeId: string
    regionInputId: string
    inputLevelId: string
    costElementFilterIds: string[],
    inputLevelFilterIds: string[]
}