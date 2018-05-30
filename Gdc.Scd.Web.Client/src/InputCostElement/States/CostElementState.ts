import { SelectList } from "../../Common/States/SelectList";
import { CostBlockInput } from "./CostBlock";
import { NamedId } from "../../Common/States/NamedId";

export interface InputLevel extends NamedId {
    filter: {
        name: string
        items: NamedId[]
    }
}

export interface CostElementMeta extends NamedId {
    dependency: NamedId
    description: string
}

export interface CostBlockMeta extends NamedId {
    applicationId: string
    costElements: CostElementMeta[]
}

export interface CostElementInput {
    applications: SelectList<NamedId>
    scopes: SelectList<NamedId>
    countries: NamedId[]
    inputLevels: InputLevel[]
    costBlocks: SelectList<CostBlockInput>
    costBlockMetas: CostBlockMeta[]
}