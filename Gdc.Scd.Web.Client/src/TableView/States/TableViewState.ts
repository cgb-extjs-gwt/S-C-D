import { NamedId } from "../../Common/States/CommonStates";
import { FieldInfo } from "../../Common/States/FieldInfo";
import { TableViewRecord } from "./TableViewRecord";
import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";

export interface TableViewRecordInfo {
    coordinates: FieldInfo[]
    data: FieldInfo[]
}

export interface TableViewInfo {
    recordInfo: TableViewRecordInfo
    filters: { [key: string]: NamedId[] }
    references: { [key: string]: NamedId[] }
}

export interface QualityGateResultSetItem {
    costElementIdentifier: CostElementIdentifier
    qualityGateResult: QualityGateResult
}

export interface QualityGateResultSet {
    items: QualityGateResultSetItem[]
    hasErros: boolean
}

export interface TableViewState {
    info: TableViewInfo
    editedRecords: TableViewRecord[]
    qualityGateResultSet: QualityGateResultSet
}