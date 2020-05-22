import { QualityGateResultSet } from "./TableViewState";

export interface ImportResult {
    errors: string[]
    qualityGateResult: QualityGateResultSet
}