import { CostElementIdentifier } from "./CostElementIdentifier";

export interface Context extends CostElementIdentifier {
    regionInputId: number
    inputLevelId: string
    costElementFilterIds: number[],
    inputLevelFilterIds: number[]
}