import { SelectList, NamedId } from "../../Common/States/CommonStates";

export interface CostImportState {
    applicationId: string
    costBlockId: string
    costElementId: string
    dependencyItems: SelectList<NamedId<number>, number>
    regions: SelectList<NamedId<number>, number>
    status: string[]
    fileName: string
}