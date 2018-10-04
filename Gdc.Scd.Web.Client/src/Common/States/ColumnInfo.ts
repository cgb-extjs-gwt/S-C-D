import { NamedId } from "./CommonStates";
import { Model } from "./ExtStates";

export enum ColumnType {
    Text,
    CheckBox,
    Numeric,
    Reference
}

export interface ColumnInfo<T=any> {
    dataIndex: string
    title: string
    type: ColumnType
    isEditable?: boolean
    referenceItems?: Map<string, NamedId>
    isInvisible?: boolean
    mappingFn?(data: T): any
    editMappingFn?(data: Model<T>, dataIndex: string)
    rendererFn?(value, record: Model<T>): any
}