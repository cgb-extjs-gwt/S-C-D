import { NamedId } from "../../Common/States/CommonStates";

export interface TableViewCostBlockInfo {
    metaId: string
    costElementIds: string[]
}

export interface TableViewInfo {
    costBlockInfos: TableViewCostBlockInfo[]
    filters: { [key: string]: NamedId[] }
    references: { [key: string]: NamedId[] }
}

export interface TableViewState {
    info: TableViewInfo
}