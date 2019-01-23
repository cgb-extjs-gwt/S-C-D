import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";

export interface ImportResult {
    errors: string[]
    qualityGateResult: QualityGateResult
}