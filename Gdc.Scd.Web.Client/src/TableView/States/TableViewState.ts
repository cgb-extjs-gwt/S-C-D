import { NamedId } from "../../Common/States/CommonStates";
import { TableViewRecord } from "./TableViewRecord";
import { QualityGateResult } from "../../QualityGate/States/QualityGateResult";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";

export interface DataInfo extends CostElementIdentifier {
    dependencyItemId?: number
    dataIndex: string
}

export interface TableViewRecordInfo {
    coordinates: string[]
    data: DataInfo[]
    additionalData: {
        dataIndex: string
        title: string
    }[]
}

export interface TableViewInfo {
    recordInfo: TableViewRecordInfo
    costBlockReferences: { 
        [costBlockId: string]: {
            references: { 
                [costElementId: string]: NamedId<number>[] 
            }
        }
    }
    dependencyItems: {
        [costElementId: string]: {
            [dependencyItemId: number]: NamedId<number>
        }
    }
}

export interface QualityGateResultSetItem {
    costElementIdentifier: CostElementIdentifier
    qualityGateResult: QualityGateResult
}

export interface QualityGateResultSet {
    items: QualityGateResultSetItem[]
    hasErrors: boolean
}

export interface TableViewState {
    info: TableViewInfo
    editedRecords: TableViewRecord[]
    qualityGateResultSet: QualityGateResultSet
}