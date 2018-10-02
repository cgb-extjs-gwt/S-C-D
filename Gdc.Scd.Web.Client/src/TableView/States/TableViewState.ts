import { NamedId } from "../../Common/States/CommonStates";
import { FieldInfo } from "../../Common/States/FieldInfo";

export interface TableViewRecordInfo {
    coordinates: FieldInfo[]
    data: FieldInfo[]
}

export interface TableViewInfo {
    recordInfo: TableViewRecordInfo
    filters: { [key: string]: NamedId[] }
    references: { [key: string]: NamedId[] }
}

export interface TableViewState {
    info: TableViewInfo
}