import { NamedId } from "./CommonStates";

export enum ColumnType {
    Text,
    CheckBox,
    Numeric,
    Reference
}

export interface ColumnInfo {
    dataIndex: string
    title: string
    type: ColumnType
    isEditable?: boolean
    referenceItems?: Map<string, NamedId>
    mappingFn?(data): any
}