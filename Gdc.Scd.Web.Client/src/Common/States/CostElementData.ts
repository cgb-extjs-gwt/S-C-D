import { NamedId } from "./CommonStates";

export interface CostElementData {
    regions: NamedId<number>[]
    dependencyItems: NamedId<number>[]
}