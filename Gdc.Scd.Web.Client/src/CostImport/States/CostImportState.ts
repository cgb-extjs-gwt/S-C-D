import { SelectList, NamedId } from "../../Common/States/CommonStates";
import { BundleDetailGroup } from "../../QualityGate/States/QualityGateResult";

export interface FileData {
    name: string
    base64Data: string
}

export interface CostImportState {
    applicationId: string
    costBlockId: string
    costElementId: string
    dependencyItems: SelectList<NamedId<number>, number>
    regions: SelectList<NamedId<number>, number>
    status: string[]
    file: FileData
    qualityGateErrors: BundleDetailGroup[]
}